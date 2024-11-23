using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.PayloadStore;
using WebhookFunctionApp.Services.RequestValidation;
using WebhookFunctionApp.Utilities;

namespace WebhookFunctionApp.Functions;

public class ValidateAndStoreFunction(
    ILoggerFactory loggerFactory,
    IRequestValidator requestValidator,
    IPayloadStore payloadStore)
{
    private readonly ILogger _logger = 
        loggerFactory.CreateLogger<ValidateAndStoreFunction>();
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IPayloadStore _payloadStore = payloadStore;

    private const string MESSAGE_ID_CUSTOM_HEADER = "10PIAC-Message-Id";

    [Function(nameof(ValidateAndStoreFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", 
            Route = "handle/contract/{contractId}/sender/{senderId}/tenant/{tenantId}"
        )] HttpRequestData request,
        string contractId,
        string senderId,
        string tenantId)
    {
        _logger.LogInformation("{functionName} Version: 241112-1813", nameof(ValidateAndStoreFunction));

        try
        {
            var messageId = GetMessageId();

            _logger.LogInformation(
                "FUNCTION_START: " +
                "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}], messageId=[{messageId}])",
                contractId, senderId, tenantId, messageId);

            var requestHeaders = GetHeaders(request);
            var requestBodyJson = 
                StreamToStringConverter.ConvertStreamToString(request.Body);

            var validationResult = 
                _requestValidator.Validate(contractId, requestBodyJson);

            HttpResponseData response =
                validationResult.IsValid
                    ? await HandleValidRequestAsync(
                        request, requestHeaders, requestBodyJson, contractId, senderId, 
                        tenantId, messageId)
                    : await HandleInvalidRequestAsync(
                        request, requestHeaders, requestBodyJson, contractId, senderId, 
                        tenantId, messageId, validationResult.ErrorMessages);

            _logger.LogInformation(
                "FUNCTION_END: => {StatusCode} " +
                "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}], messageId=[{messageId}])",
                response.StatusCode, contractId, senderId, tenantId, 
                messageId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FUNCTION_EXCEPTION: {exceptionType} [{exceptionMessage}] " +
                "(contractId=[{contractId}], senderId=[{senderId}], tenantId=[{tenantId}])",
                ex.GetType().FullName, ex.Message, contractId, senderId, 
                tenantId);
            throw;
        }
    }

    private static Dictionary<string, IEnumerable<string>> GetHeaders(
        HttpRequestData request)
    {
        var requestHeaders =
            request.Headers
                .Where(header =>
                    header.Key != "x-functions-key")
                .ToDictionary(
                    header => header.Key,
                    header => header.Value,
                    StringComparer.OrdinalIgnoreCase  // Ensures the dictionary is case-insensitive
                );
        return requestHeaders;
    }

    private async Task<HttpResponseData> HandleValidRequestAsync(
        HttpRequestData request,
        Dictionary<string, IEnumerable<string>> requestHeaders,
        string requestBodyJson,
        string contractId,
        string senderId,
        string tenantId,
        string messageId)
    {
        _logger.LogDebug("{method} called", nameof(HandleValidRequestAsync));

        await _payloadStore.AddAcceptedPayloadAsync(
            tenantId, senderId, contractId, messageId, requestHeaders, requestBodyJson);

        HttpResponseData response = request.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.Headers.Add(MESSAGE_ID_CUSTOM_HEADER, messageId);
        response.WriteString("Created");
        return response;
    }

    private async Task<HttpResponseData> HandleInvalidRequestAsync(
        HttpRequestData req,
        Dictionary<string, IEnumerable<string>> requestHeaders,
        string requestBodyJson,
        string contractId,
        string senderId,
        string tenantId,
        string messageId,
        IList<string>? errorMessages)
    {
        await _payloadStore.AddRejectedPayloadAsync(
            tenantId, senderId, contractId, messageId, requestHeaders, requestBodyJson, errorMessages);

        var responseBodyJson = JsonSerializer.Serialize(errorMessages ?? [], options: new() { WriteIndented = true });

        HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add(MESSAGE_ID_CUSTOM_HEADER, messageId);
        response.WriteString(responseBodyJson);

        return response;
    }

    private static string GetMessageId()
    {
        return $"{DateTime.UtcNow:s}UTC-{Guid.NewGuid()}";
    }
}
