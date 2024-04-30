using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Models;

public abstract class PayloadBase(
    string tenantId,
    string senderId,
    string contractId,
    string messageId,
    IDictionary<string, IEnumerable<string>> headers,
    string body)
{
    public string TenantId { get; } = tenantId;
    public string SenderId { get; } = senderId;
    public string ContractId { get; } = contractId;
    public string MessageId { get; } = messageId;
    public IDictionary<string, IEnumerable<string>> Headers { get; set; } = headers;

    public string Body { get; set; } = body;
}
