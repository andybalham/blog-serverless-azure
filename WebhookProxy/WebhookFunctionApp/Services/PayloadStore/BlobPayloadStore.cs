using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Services.RequestStore;

public class BlobPayloadStore(ILoggerFactory loggerFactory) : IPayloadStore
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<BlobPayloadStore>();

    private const string CONTAINER_NAME_ACCEPTED_PAYLOADS = "webhook-payloads-accepted";
    private const string CONTAINER_NAME_REJECTED_PAYLOADS = "webhook-payloads-rejected";

    // TODO: Get this from the environment, then key vault
    private readonly string connectionString = "UseDevelopmentStorage=true"; // Local emulator connection string

    /*
     Have two containers, but same folders:
        - `/{tenantId}/{senderId}/{contractId}/{yyyy-mm-dd}/{HH-mm-ss}UTC-{messageId}.json`
     */

    public async Task AddRejectedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody,
        IList<string>? errorMessages)
    {
        // TODO: Implement this
        _logger.LogDebug($"{nameof(AddRejectedPayloadAsync)} called");


    }

    public async Task AddAcceptedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody)
    {
        // TODO: Implement this
        _logger.LogDebug($"{nameof(AddAcceptedPayloadAsync)} called");

        // TODO: Build up the JSON to store
        string payloadJsonString = "{\"name\":\"John Doe\",\"age\":30}";

        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(CONTAINER_NAME_ACCEPTED_PAYLOADS);

        string blobName = GetBlobName(tenantId, senderId, contractId, messageId);
        
        var blobClient = containerClient.GetBlobClient(blobName);

        byte[] byteArray = Encoding.UTF8.GetBytes(payloadJsonString);
        using var stream = new MemoryStream(byteArray);
        
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    private static string GetBlobName(
        string tenantId,
        string senderId,
        string contractId,
        string messageId)
    {
        var now = DateTime.UtcNow;
        var blobName = $"{tenantId}/{senderId}/{contractId}/{now:yyyy-MM-dd}/{now:HHmm}UTC-{messageId}.json";
        return blobName;
    }
}
