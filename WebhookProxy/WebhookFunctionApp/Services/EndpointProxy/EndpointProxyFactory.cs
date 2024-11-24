using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Services.EndpointProxy;

public class EndpointProxyFactory(ILogger<EndpointProxyFactory> logger) : IEndpointProxyFactory
{
    public IEndpointProxy GetEndpointProxy(string tenantId, string contractId)
    {
        return new MockEndpointProxy(logger, tenantId, contractId);
    }
}
