using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Models;

public class RejectedPayload(
    IDictionary<string, IEnumerable<string>> headers,
    string body,
    IList<string>? errorMessages) : PayloadBase(headers, body)
{
    public IList<string>? ErrorMessages { get; set; } = errorMessages;
}
