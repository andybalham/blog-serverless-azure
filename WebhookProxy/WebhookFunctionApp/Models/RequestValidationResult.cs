namespace WebhookFunctionApp.Models;

public class RequestValidationResult
{
    public bool IsValid { get; set; }
    public IList<string>? ErrorMessages { get; set; }
}