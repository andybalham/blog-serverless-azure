namespace WebhookFunctionApp.Services.RequestStore;

public interface IPayloadStore
{
    Task AddAcceptedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody);

    Task AddRejectedPayloadAsync(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody,
        IList<string>? errorMessages);
}