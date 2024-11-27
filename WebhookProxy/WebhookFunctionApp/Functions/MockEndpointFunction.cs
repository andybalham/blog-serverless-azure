using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using WebhookFunctionApp.Utilities;

namespace WebhookFunctionApp.Functions;

public class MockEndpointFunction
{
    private readonly ILogger<MockEndpointFunction> _logger;

    public MockEndpointFunction(ILogger<MockEndpointFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(MockEndpointFunction))]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Function, "post",
        Route = "mock-endpoint/tenant/{tenantId}/contract/{contractId}"
    )] HttpRequestData request,
        string tenantId,
        string senderId)
    {
        _logger.LogInformation("{functionName} Version: 241124-0944", nameof(MockEndpointFunction));

        try
        {
            _logger.LogInformation(
                "FUNCTION_START: " +
                "(tenantId=[{tenantId}], senderId=[{senderId}])",
                senderId, tenantId
                );

            var requestBodyJson =
                StreamToStringConverter.ConvertStreamToString(request.Body);

            _logger.LogInformation("{requestBodyJson}", requestBodyJson);

            // TODO: What do we need to be able to mock?
            /*
             * 
             */

            var response = request.CreateResponse(HttpStatusCode.Accepted);

            _logger.LogInformation(
                "FUNCTION_END: => {StatusCode} " +
                "(tenantId=[{tenantId}], senderId=[{senderId}])",
                response.StatusCode, senderId, tenantId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FUNCTION_EXCEPTION: {exceptionType} [{exceptionMessage}] " +
                "(tenantId=[{tenantId}], senderId=[{senderId}])",                
                ex.GetType().FullName, ex.Message, senderId, tenantId);
            throw;
        }
    }
}
