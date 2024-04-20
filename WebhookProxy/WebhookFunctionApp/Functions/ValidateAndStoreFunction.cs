using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebhookFunctionApp.Services.RequestStore;
using WebhookFunctionApp.Services.RequestValidation;
using WebhookFunctionApp.Utilities;

namespace WebhookFunctionApp.Functions;

public class ValidateAndStoreFunction(
    ILoggerFactory loggerFactory,
    IRequestValidator requestValidator,
    IRequestStore requestStore)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ValidateAndStoreFunction>();
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IRequestStore _requestStore = requestStore;

    private const string FUNCTION_NAME = "ValidateAndStore";
    private const string MESSAGE_ID_CUSTOM_HEADER = "10PIAC-Message-Id";

    [Function(FUNCTION_NAME)]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", 
            Route = "handle/contract/{contractId}/sender/{senderId}/tenant/{tenantId}")] HttpRequestData req,
        string contractId, 
        string senderId, 
        string tenantId)
    {
        var messageId = GetMessageId();

        _logger.LogInformation(
            "FUNCTION_START: {FunctionName} " +
            "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}], messageId=[{messageId}])",
            FUNCTION_NAME, contractId, senderId, tenantId, messageId);

        var requestBodyJson = StreamToStringConverter.ConvertStreamToString(req.Body);

        var validationResult = _requestValidator.Validate(contractId, requestBodyJson);

        HttpResponseData response =
            validationResult.IsValid
                ? HandleValidRequest(req, contractId, senderId, tenantId, messageId)
                : HandleInvalidRequest(
                    req, contractId, senderId, tenantId, messageId, validationResult.ErrorMessages);

        _logger.LogInformation(
            "FUNCTION_END: {FunctionName} => {StatusCode} " +
            "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}], messageId=[{messageId}])",
            FUNCTION_NAME, response.StatusCode, contractId, senderId, tenantId, messageId);

        return response;
    }

    private HttpResponseData HandleValidRequest(HttpRequestData req,
                                                string contractId,
                                                string senderId,
                                                string tenantId,
                                                string messageId)
    {
        var requestHeaders =
            req.Headers.ToList().Select(h =>
                new Tuple<string, string>(h.Key, string.Join(", ", h.Value.ToArray())));
        var requestBody = StreamToStringConverter.ConvertStreamToString(req.Body);

        _requestStore.PutValidRequest(tenantId, senderId, contractId, messageId, requestHeaders, requestBody);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.Headers.Add(MESSAGE_ID_CUSTOM_HEADER, messageId);
        response.WriteString("Created");
        return response;
    }

    private HttpResponseData HandleInvalidRequest(HttpRequestData req,
                                                  string contractId,
                                                  string senderId,
                                                  string tenantId,
                                                  string messageId,
                                                  IList<string>? errorMessages)
    {
        var requestHeaders =
            req.Headers.ToList().Select(h =>
                new Tuple<string, string>(h.Key, string.Join(", ", h.Value.ToArray())));
        var requestBody = StreamToStringConverter.ConvertStreamToString(req.Body);

        _requestStore.PutInvalidRequest(
            tenantId, senderId, contractId, messageId, requestHeaders, requestBody, errorMessages);

        var responseBodyJson = JsonConvert.SerializeObject(errorMessages ?? [], Formatting.Indented);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add(MESSAGE_ID_CUSTOM_HEADER, messageId);
        response.WriteString(responseBodyJson);

        return response;
    }

    private static string GetMessageId()
    {
        return Guid.NewGuid().ToString();
    }
}
