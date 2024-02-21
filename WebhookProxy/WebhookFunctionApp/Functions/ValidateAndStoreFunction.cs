using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.RequestValidation;

namespace WebhookFunctionApp.Functions
{
    public class ValidateAndStoreFunction
    {
        private readonly ILogger _logger;
        private readonly IRequestValidator _requestValidator;

        public ValidateAndStoreFunction(ILoggerFactory loggerFactory, IRequestValidator requestValidator)
        {
            _logger = loggerFactory.CreateLogger<ValidateAndStoreFunction>();
            _requestValidator = requestValidator;
        }

        [Function("validate-and-store")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, 
                "post", Route = "handle/contract/{contractId}/sender/{senderId}/tenant/{tenantId}"
            )] HttpRequestData req,
            string contractId, string senderId, string tenantId)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var validationResult = _requestValidator.Validate(contractId, req.Body);

            if (validationResult.Invalid)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                response.WriteString("Bad Request");
                return response;
            }
            else
            {
                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                response.WriteString("Created");
                return response;
            }
        }
    }
}
