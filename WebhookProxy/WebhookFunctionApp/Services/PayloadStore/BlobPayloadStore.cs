using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using WebhookFunctionApp.Models;
using WebhookFunctionApp.Services.BlobService;
using WebhookFunctionApp.Services.PayloadStore;

namespace WebhookFunctionApp.Services.RequestStore;

public class BlobPayloadStore : IPayloadStore
{
    private readonly ILogger _logger;

    private const string CONTAINER_NAME_ACCEPTED_PAYLOADS = "webhook-payloads-accepted";
    private const string CONTAINER_NAME_REJECTED_PAYLOADS = "webhook-payloads-rejected";

    private readonly BlobServiceClient _blobServiceClient;

    public BlobPayloadStore(
        IBlobServiceClientFactory blobServiceClientFactory, 
        ILoggerFactory loggerFactory)
    {
        _blobServiceClient = blobServiceClientFactory.CreateClient();
        _logger = loggerFactory.CreateLogger<BlobPayloadStore>();
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

    public async Task<AcceptedPayload> GetAcceptedPayloadAsync(string blobUrl)
    {
        var blobContent = 
            await LoadBlobContentFromUrlAsync(CONTAINER_NAME_ACCEPTED_PAYLOADS, blobUrl);

        var acceptedPayload = JsonConvert.DeserializeObject<AcceptedPayload>(blobContent);

        return acceptedPayload;
    }

    private async Task<string> LoadBlobContentFromUrlAsync(string containerName, string blobUrl)
    {
        // Parse the blob URL to get the container name and blob name
        var blobUri = new Uri(blobUrl);

        var containerNameIndex = Array.IndexOf(blobUri.Segments, containerName + "/");

        if (containerNameIndex == -1)
        {
            throw new Exception($"Blob URL is not for container [{containerName}]: {blobUrl}");
        }

        var blobName = string.Join("", blobUri.Segments[(containerNameIndex + 1)..]);

        _logger.LogInformation("blobName: {blobName}", blobName);

        // Get a reference to the blob
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        // Download the blob content as a stream
        BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();

        // Read the stream into a string
        using var reader = new StreamReader(downloadInfo.Content);

        string blobContent = await reader.ReadToEndAsync();
        return blobContent;
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
