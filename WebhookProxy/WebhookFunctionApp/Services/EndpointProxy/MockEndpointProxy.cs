using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Contracts;

namespace WebhookFunctionApp.Services.EndpointProxy;

public class MockEndpointProxy(
    ILogger _logger,
    ApiClient _apiClient,
    IConfiguration _configuration,
    string _tenantId, 
    string _contractId) 
    : IEndpointProxy
{
    public async Task InvokeAsync(string payload)
    {
        _logger.LogDebug("{className} handling payload for tenantId [{tenantId}], contractId [{contractId}]: [{payload}] ",
            nameof(MockEndpointProxy), _tenantId, _contractId, payload);

        var mockEndpointUrlBase = 
            _configuration["MockEndpointUrlBase"]
            ?? throw new Exception("MockEndpointUrlBase not specified");

        var mockEndpointApiKey = 
            _configuration["MockEndpointApiKey"] 
            ?? throw new Exception("MockEndpointApiKey not specified");

        var headers = new Dictionary<string, string>
        {
            { "x-functions-key", mockEndpointApiKey }
        };

        var mockEndpointUrl = 
            mockEndpointUrlBase + $"/mock-endpoint/tenant/{_tenantId}/contract/{_contractId}";

        var response = await _apiClient.PostJsonWithFactoryAsync(mockEndpointUrl, payload, headers);

        _logger.LogDebug("Response: {response}", response);
    }
}
