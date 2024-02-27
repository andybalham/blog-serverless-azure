using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.RequestStorage;
using WebhookFunctionApp.Services.RequestValidation;
using static WebhookFunctionApp.Services.RequestValidation.RequestValidator;

namespace WebhookFunctionApp.Functions
{
    public class ValidateAndStoreFunction
    {
        private const string FUNCTION_NAME = "ValidateAndStore";

        private readonly ILogger _logger;
        private readonly IRequestValidator _requestValidator;
        private readonly IRequestStore _requestStore;

        public ValidateAndStoreFunction(ILoggerFactory loggerFactory, IRequestValidator requestValidator, IRequestStore requestStore)
        {
            _logger = loggerFactory.CreateLogger<ValidateAndStoreFunction>();
            _requestValidator = requestValidator;
            _requestStore = requestStore;
        }

        [Function(FUNCTION_NAME)]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Function, 
                "post", Route = "handle/contract/{contractId}/sender/{senderId}/tenant/{tenantId}"
            )] HttpRequestData req,
            string contractId, string senderId, string tenantId)
        {
            _logger.LogInformation(
                "FUNCTION_START: {FunctionName} (contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}])",
                FUNCTION_NAME, contractId, senderId, tenantId);

            var requestBodyJson = StreamToStringConverter.ConvertStreamToString(req.Body);

            var validationResult = _requestValidator.Validate(contractId, requestBodyJson);

            HttpResponseData response =
                validationResult.IsValid
                    ? HandleValidRequest(req, contractId, senderId, tenantId)
                    : HandleInvalidRequest(req, contractId, senderId, tenantId, validationResult.ErrorMessages);

            _logger.LogInformation(
                "FUNCTION_END: {FunctionName} => {StatusCode} (contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}])",
                FUNCTION_NAME, response.StatusCode, contractId, senderId, tenantId);

            return response;
        }

        private HttpResponseData HandleValidRequest(
            HttpRequestData req, string contractId, string senderId, string tenantId)
        {
            _requestStore.PutValidRequest(req, contractId, senderId, tenantId);

            HttpResponseData response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Created");
            return response;
        }

        private HttpResponseData HandleInvalidRequest(
            HttpRequestData req, string contractId, string senderId, string tenantId, IList<string> errorMessages)
        {
            _requestStore.PutInvalidRequest(req, contractId, senderId, tenantId, errorMessages);

            HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Bad Request");
            return response;
        }
    }
}
