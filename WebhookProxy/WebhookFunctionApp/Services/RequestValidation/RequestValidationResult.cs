
namespace WebhookFunctionApp.Services.RequestValidation
{
    public class RequestValidationResult
    {
        public bool Invalid { get; set; }
        public IList<string> ErrorMessages { get; internal set; }
    }
}