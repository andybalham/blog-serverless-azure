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

    /*
     Have two containers:

        - ValidWebhookPayloads: `/{tenantId}/{senderId}/{contractId}/{yyyy-mm-dd}/{messageId}.json`
        - InvalidWebhookPayloads: `/{tenantId}/{senderId}/{contractId}/{yyyy-mm-dd}/{messageId}.json`

        We will return `messageId` as a custom header `10piac-message-id`.

        We will log out a message to link the `messageId` to the location of the payload.
     */

    public void PutInvalidRequest(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody,
        IList<string>? errorMessages)
    {
        // TODO: Implement this
        _logger.LogDebug($"{nameof(PutInvalidRequest)} called");
    }

    public void PutValidRequest(
        string tenantId,
        string senderId,
        string contractId,
        string messageId,
        IEnumerable<Tuple<string, string>> requestHeaders,
        string requestBody)
    {
        // TODO: Implement this
        _logger.LogDebug($"{nameof(PutValidRequest)} called");
    }
}
