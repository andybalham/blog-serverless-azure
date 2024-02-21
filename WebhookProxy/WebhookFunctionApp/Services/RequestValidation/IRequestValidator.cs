namespace WebhookFunctionApp.Services.RequestValidation
{
    public interface IRequestValidator
    {
        RequestValidationResult Validate(string contractId, Stream requestBodyStream);
    }
}