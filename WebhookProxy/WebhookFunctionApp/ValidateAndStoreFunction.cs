using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace WebhookFunctionApp
{
    public class ValidateAndStoreFunction
    {
        private readonly ILogger _logger;

        public ValidateAndStoreFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidateAndStoreFunction>();
        }

        [Function("validate-and-store")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, 
                "post", Route = "handle/contract/{contractId}/sender/{senderId}/tenant/{tenantId}"
            )] HttpRequestData req,
            string contractId, string senderId, string tenantId)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
