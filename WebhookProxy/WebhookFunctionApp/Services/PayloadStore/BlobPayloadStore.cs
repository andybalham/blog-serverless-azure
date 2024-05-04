using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookFunctionApp.Models;

namespace WebhookFunctionApp.Services.RequestStore;

public class BlobPayloadStore : IPayloadStore
{
    private readonly ILogger _logger;

    private const string CONTAINER_NAME_ACCEPTED_PAYLOADS = "webhook-payloads-accepted";
    private const string CONTAINER_NAME_REJECTED_PAYLOADS = "webhook-payloads-rejected";

    private readonly BlobServiceClient _blobServiceClient;

    public BlobPayloadStore(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<BlobPayloadStore>();

        // TODO: Local emulator connection string, get from environment, then key vault
        _blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
    }

    public async Task AddRejectedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IDictionary<string, IEnumerable<string>> requestHeaders,
        string requestBody,
        IList<string>? errorMessages)
    {
        _logger.LogDebug($"{nameof(AddRejectedPayloadAsync)} called");

        var payload = 
            new RejectedPayload(
                tenantId, senderId, contractId, messageId, requestHeaders, requestBody, 
                errorMessages);

        await UploadPayloadAsync(
            CONTAINER_NAME_REJECTED_PAYLOADS, 
            tenantId, senderId, contractId, messageId, payload);
    }

    public async Task AddAcceptedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IDictionary<string, IEnumerable<string>> requestHeaders,
        string requestBody)
    {
        _logger.LogDebug($"{nameof(AddAcceptedPayloadAsync)} called");

        var payload = 
            new AcceptedPayload(
                tenantId, senderId, contractId, messageId, requestHeaders, requestBody); 

        await UploadPayloadAsync(
            CONTAINER_NAME_ACCEPTED_PAYLOADS, 
            tenantId, senderId, contractId, messageId, payload);
    }

    private async Task UploadPayloadAsync<T>(
        string containerName,
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        T payload) where T : PayloadBase
    {
        string payloadJsonString = 
            JsonConvert.SerializeObject(payload, Formatting.Indented);

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var blobName = GetBlobName(tenantId, senderId, messageId);

        var blobClient = containerClient.GetBlobClient(blobName);

        var byteArray = Encoding.UTF8.GetBytes(payloadJsonString);
        using var stream = new MemoryStream(byteArray);

        await blobClient.UploadAsync(stream, overwrite: true);
    }

    private static string GetBlobName(
        string tenantId,
        string senderId,
        string messageId)
    {
        var blobName = 
            $"{tenantId}/{senderId}/{DateTime.UtcNow:yyyy-MM-dd}/{messageId}.json";
        return blobName;
    }
}
