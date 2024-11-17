// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}

using System;
using System.Text.Json;
using Azure.Messaging;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.PayloadStore;
using System.Diagnostics.Contracts;

namespace WebhookFunctionApp.Functions;

public class DedupeAndForwardFunction
{
    private readonly ILogger<DedupeAndForwardFunction> _logger;

    private const string FUNCTION_NAME = "DedupeAndForward";

    public DedupeAndForwardFunction(ILogger<DedupeAndForwardFunction> logger)
    {
        _logger = logger;
    }

    [Function(FUNCTION_NAME)]
    public void Run([EventGridTrigger] CloudEvent cloudEvent) // TODO: Mention this is the default
    {
        _logger.LogDebug("{FunctionName} Version: 241117-1644", FUNCTION_NAME);

        try
        {
            _logger.LogInformation(
                "FUNCTION_START: {functionName} " +
                "Event type: {type}, Event subject: {subject}",
                FUNCTION_NAME, cloudEvent.Type, cloudEvent.Subject);

            _logger.LogDebug(
                "{functionName} Event: {event}",
                FUNCTION_NAME, JsonSerializer.Serialize(cloudEvent));

            // Code from: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.eventgrid-readme?view=azure-dotnet#deserializing-event-data

            if (cloudEvent.TryGetSystemEventData(out object systemEvent))
            {
                switch (systemEvent)
                {
                    case SubscriptionValidationEventData subscriptionValidated:
                        _logger.LogDebug(
                            "{functionName} subscriptionValidated.ValidationCode: {code}",
                            FUNCTION_NAME, subscriptionValidated.ValidationCode);
                        break;

                    case StorageBlobCreatedEventData blobCreated:
                        _logger.LogDebug(
                            "{functionName} blobCreated.Url: {url}", 
                            FUNCTION_NAME, blobCreated.Url);
                        //var acceptedPayload = await _payloadStore.GetAcceptedPayloadAsync(blobCreated.Url);
                        //_logger.LogInformation("acceptedPayload: {acceptedPayload}", JsonSerializer.Serialize(acceptedPayload));
                        break;

                    default:
                        _logger.LogError(
                            "{functionName} Unhandled event type: {eventType}",
                            FUNCTION_NAME, cloudEvent.Type);
                        break;
                }
            }
            else
            {
                _logger.LogError(
                    "{functionName} Unhandled event type: {eventType}",
                    FUNCTION_NAME, cloudEvent.Type);
            }

            _logger.LogInformation(
                "FUNCTION_END: {functionName} " +
                "Event type: {type}, Event subject: {subject}",
                FUNCTION_NAME, cloudEvent.Type, cloudEvent.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FUNCTION_EXCEPTION: {functionName} {exceptionType} [{exceptionMessage}] " +
                "Event type: {type}, Event subject: {subject}",
                FUNCTION_NAME, ex.GetType().FullName, ex.Message, cloudEvent.Type, cloudEvent.Subject);
            throw;
        }
    }
}
