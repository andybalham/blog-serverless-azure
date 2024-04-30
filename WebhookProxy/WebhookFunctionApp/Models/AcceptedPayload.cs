using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Models;

public class AcceptedPayload(
    string tenantId,
    string senderId,
    string contractId,
    string messageId,
    IDictionary<string, IEnumerable<string>> headers,
    string body) : PayloadBase(tenantId, senderId, contractId, messageId, headers, body)
{
}
