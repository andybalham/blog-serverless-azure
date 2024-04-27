namespace WebhookFunctionApp.Services.RequestStore;

public interface IPayloadStore
{
    Task AddAcceptedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IDictionary<string, IEnumerable<string>> requestHeaders,
        string requestBody);

    Task AddRejectedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IDictionary<string, IEnumerable<string>> requestHeaders,
        string requestBody,
        IList<string>? errorMessages);
}