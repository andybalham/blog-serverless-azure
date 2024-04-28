using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookFunctionApp.Models;

public abstract class PayloadBase(IDictionary<string, IEnumerable<string>> headers, string body)
{
    public IDictionary<string, IEnumerable<string>> Headers { get; set; } = headers;

    public string Body { get; set; } = body;
}
