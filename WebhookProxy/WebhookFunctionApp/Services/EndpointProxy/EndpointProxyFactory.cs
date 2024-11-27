using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Services.EndpointProxy;

public class EndpointProxyFactory(
    ILogger<EndpointProxyFactory> _logger,
    ApiClient _apiClient,
    IConfiguration _configuration) : IEndpointProxyFactory
{
    public IEndpointProxy GetEndpointProxy(string tenantId, string contractId)
    {
        return new MockEndpointProxy(_logger, _apiClient, _configuration, tenantId, contractId);
    }
}
