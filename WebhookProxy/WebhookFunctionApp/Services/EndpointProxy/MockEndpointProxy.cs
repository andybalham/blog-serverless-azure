using Microsoft.Extensions.Logging;
using System.Diagnostics.Contracts;

namespace WebhookFunctionApp.Services.EndpointProxy;

public class MockEndpointProxy(
    ILogger logger, 
    string tenantId, 
    string contractId) 
    : IEndpointProxy
{
    public Task InvokeAsync(string payload)
    {
        logger.LogDebug("{className} handling payload for tenantId [{tenantId}], contractId [{contractId}]: [{payload}] ",
            nameof(MockEndpointProxy), tenantId, contractId, payload);
        return Task.CompletedTask;
    }
}
