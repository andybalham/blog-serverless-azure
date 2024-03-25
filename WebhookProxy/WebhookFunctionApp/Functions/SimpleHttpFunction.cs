using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace WebhookFunctionApp.Functions;

public class SimpleHttpFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SimpleHttpFunction>();

    //[Function("HttpExample")]
    public HttpResponseData Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString("Welcome to Azure Functions!");
        return response;
    }

    //[Function("SimpleHttpFunction")]
    public HttpResponseData Run_(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "simplefunction")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Welcome to Azure Functions!");

        return response;
    }
}