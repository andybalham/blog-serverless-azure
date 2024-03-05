namespace WebhookFunctionApp.Services.RequestStore;

public interface IRequestStore
{
    void PutValidRequest(
        IEnumerable<Tuple<string, string>> requestHeaders, string requestBody, string contractId, string senderId, string tenantId);

    void PutInvalidRequest(
        IEnumerable<Tuple<string, string>> requestHeaders, string requestBody, string contractId, string senderId, string tenantId, 
        IList<string>? errorMessages);
}