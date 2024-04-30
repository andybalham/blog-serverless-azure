using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Models;

public class RejectedPayload(
    string tenantId,
    string senderId,
    string contractId,
    string messageId,
    IDictionary<string, IEnumerable<string>> headers,
    string body,
    IList<string>? errorMessages) : PayloadBase(tenantId, senderId, contractId, messageId, headers, body)
{
    public IList<string>? ErrorMessages { get; set; } = errorMessages;
}
