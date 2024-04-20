namespace WebhookFunctionApp.Services.RequestStore;

public interface IRequestStore
{
    void PutValidRequest(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody);

    void PutInvalidRequest(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody,
        IList<string>? errorMessages);
}