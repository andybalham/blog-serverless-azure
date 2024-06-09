using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookFunctionApp.Services.RequestStore;

namespace WebhookFunctionApp.Services.BlobService;

public class BlobServiceClientFactory : IBlobServiceClientFactory
{
    private readonly ILogger _logger;
    private readonly TokenCredential _tokenCredential;

    public BlobServiceClientFactory(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<BlobPayloadStore>();
        _tokenCredential = new ManagedIdentityCredential();
    }

    public BlobServiceClient CreateClient()
    {
        BlobServiceClient blobServiceClient;

        if (Environment.GetEnvironmentVariable(
            "AZURE_STORAGE_EMULATOR_RUNNING") == "true")
        {
            // Use connection string for Azurite
            string connectionString = "UseDevelopmentStorage=true";
            blobServiceClient = new BlobServiceClient(connectionString);
            
            _logger.LogDebug("Using connection string for Azurite");
        }
        else
        {
            // Use TokenCredential for Azure Storage
            var webhookStorageAccount =
                Environment.GetEnvironmentVariable("WEBHOOK_STORAGE_ACCOUNT");
            var blobServiceUri =
                new Uri($"https://{webhookStorageAccount}.blob.core.windows.net");
            blobServiceClient = 
                new BlobServiceClient(blobServiceUri, _tokenCredential);
            
            _logger.LogDebug("Using TokenCredential for Azure Storage");
        }

        return blobServiceClient;
    }
}
