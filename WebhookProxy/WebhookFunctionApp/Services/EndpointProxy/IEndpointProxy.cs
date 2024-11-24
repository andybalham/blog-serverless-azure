namespace WebhookFunctionApp.Services.EndpointProxy;

public interface IEndpointProxy
{
    Task InvokeAsync(string payload);
}
