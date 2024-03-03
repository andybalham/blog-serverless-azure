using WebhookFunctionApp.Models;

namespace WebhookFunctionApp.Services.RequestValidation;

public interface IRequestValidator
{
    RequestValidationResult Validate(string contractId, string requestBody);
}