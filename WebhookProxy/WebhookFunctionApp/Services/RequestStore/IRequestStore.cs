using Microsoft.Azure.Functions.Worker.Http;

namespace WebhookFunctionApp.Services.RequestStorage
{
    public interface IRequestStore
    {
        void PutValidRequest(HttpRequestData req, string contractId, string senderId, string tenantId);

        void PutInvalidRequest(HttpRequestData req, string contractId, string senderId, string tenantId, IList<string>? errorMessages);
    }
}