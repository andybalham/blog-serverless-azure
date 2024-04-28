using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Models;

public class AcceptedPayload(
    IDictionary<string, IEnumerable<string>> headers,
    string body) : PayloadBase(headers, body)
{
}
