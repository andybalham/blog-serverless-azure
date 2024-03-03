using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace WebhookFunctionApp.Functions;

public static class SimpleHttpFunction
{
    // TODO: Static vs. Non-static
    // TODO: FunctionContext as a parameter
    [Function("SimpleHttpFunction")]
    public static HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", 
            Route = "simplefunction")] HttpRequestData req, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("SimpleHttpFunction");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Welcome to Azure Functions!");

        return response;
    }
}