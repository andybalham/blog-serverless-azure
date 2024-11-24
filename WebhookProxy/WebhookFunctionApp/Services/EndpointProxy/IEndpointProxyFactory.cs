using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Services.EndpointProxy;

public interface IEndpointProxyFactory
{
    IEndpointProxy GetEndpointProxy(string tenantId, string contractId);
}
