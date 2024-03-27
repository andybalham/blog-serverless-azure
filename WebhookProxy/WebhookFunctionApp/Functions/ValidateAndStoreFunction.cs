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

    [Function(FUNCTION_NAME)]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", 
            Route = "handle/contract/{contractId}/sender/{senderId}/tenant/{tenantId}")] HttpRequestData req,
        string contractId, 
        string senderId, 
        string tenantId)
    {
        //try
        //{
        _logger.LogInformation(
            "FUNCTION_START: {FunctionName} " +
            "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}])",
            FUNCTION_NAME, contractId, senderId, tenantId);

        var requestBodyJson = StreamToStringConverter.ConvertStreamToString(req.Body);

        var validationResult = _requestValidator.Validate(contractId, requestBodyJson);

        HttpResponseData response =
            validationResult.IsValid
                ? HandleValidRequest(req, contractId, senderId, tenantId)
                : HandleInvalidRequest(
                    req, contractId, senderId, tenantId, validationResult.ErrorMessages);

        _logger.LogInformation(
            "FUNCTION_END: {FunctionName} => {StatusCode} " +
            "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}])",
            FUNCTION_NAME, response.StatusCode, contractId, senderId, tenantId);

        return response;
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "{ExceptionType}: {ExceptionMessage}", ex.GetType().FullName, ex.Message);
        //    throw;
        //}
    }

    private HttpResponseData HandleValidRequest(HttpRequestData req,
                                                string contractId,
                                                string senderId,
                                                string tenantId)
    {
        var requestHeaders =
            req.Headers.ToList().Select(h => 
                new Tuple<string, string>(h.Key, string.Join(", ", h.Value.ToArray())));
        var requestBody = StreamToStringConverter.ConvertStreamToString(req.Body);

        _requestStore.PutValidRequest(requestHeaders, requestBody, contractId, senderId, tenantId);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Created");
        return response;
    }

    private HttpResponseData HandleInvalidRequest(HttpRequestData req,
                                                  string contractId,
                                                  string senderId,
                                                  string tenantId,
                                                  IList<string>? errorMessages)
    {
        var requestHeaders =
            req.Headers.ToList().Select(h =>
                new Tuple<string, string>(h.Key, string.Join(", ", h.Value.ToArray())));
        var requestBody = StreamToStringConverter.ConvertStreamToString(req.Body);

        _requestStore.PutInvalidRequest(requestHeaders, requestBody, contractId, senderId, tenantId, errorMessages);

        var responseBodyJson = JsonConvert.SerializeObject(errorMessages ?? [], Formatting.Indented);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(responseBodyJson);

        return response;
    }
}
