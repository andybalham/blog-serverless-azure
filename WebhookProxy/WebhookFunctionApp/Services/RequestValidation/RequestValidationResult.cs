
namespace WebhookFunctionApp.Services.RequestValidation
{
    public class RequestValidationResult
    {
        public bool IsValid { get; set; }
        public IList<string> ErrorMessages { get; internal set; }
    }
}