using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Services.RequestStore;

public class BlobRequestStore(ILoggerFactory loggerFactory) : IRequestStore
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<BlobRequestStore>();

    // TODO: Store the requests under requests/[valid|invalid]/{senderId}/{date:yyyy-mm-dd}/{date-time:yyyymmdd-hhmm}-{tenantId}-{contractId}.json

    public void PutInvalidRequest(
        IEnumerable<Tuple<string, string>> requestHeaders, string requestBody, 
        string contractId, string senderId, string tenantId, IList<string>? errorMessages)
    {
        // TODO: Implement this
        _logger.LogDebug($"{nameof(PutInvalidRequest)} called");
    }

    public void PutValidRequest(
        IEnumerable<Tuple<string, string>> requestHeaders, string requestBody, 
        string contractId, string senderId, string tenantId)
    {
        // TODO: Implement this
        _logger.LogDebug($"{nameof(PutValidRequest)} called");
    }
}
