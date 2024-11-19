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

    public DedupeAndForwardFunction(ILogger<DedupeAndForwardFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(DedupeAndForwardFunction))]
    public void Run([EventGridTrigger] EventGridEvent eventGridEvent) // TODO: Mention this is the default
    {
        _logger.LogDebug("{FunctionName} Version: 241117-1644", nameof(DedupeAndForwardFunction));

        try
        {
            _logger.LogInformation(
                "FUNCTION_START: " +
                "Event type: {eventType}, Event subject: {subject}",
                eventGridEvent.EventType, eventGridEvent.Subject);

            _logger.LogDebug(
                "EVENT: {event}",
                JsonSerializer.Serialize(eventGridEvent));

            // Code from: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.eventgrid-readme?view=azure-dotnet#deserializing-event-data

            if (eventGridEvent.TryGetSystemEventData(out object systemEvent))
            {
                switch (systemEvent)
                {
                    case SubscriptionValidationEventData subscriptionValidated:
                        _logger.LogDebug(
                            "subscriptionValidated.ValidationCode: {code}",
                            subscriptionValidated.ValidationCode);
                        break;

                    case StorageBlobCreatedEventData blobCreated:
                        _logger.LogDebug(
                            "blobCreated.Url: {url}", 
                            blobCreated.Url);
                        //var acceptedPayload = await _payloadStore.GetAcceptedPayloadAsync(blobCreated.Url);
                        //_logger.LogInformation("acceptedPayload: {acceptedPayload}", JsonSerializer.Serialize(acceptedPayload));
                        break;

                    default:
                        _logger.LogError(
                            "Unhandled event type: {eventType}",
                            eventGridEvent.EventType);
                        break;
                }
            }
            else
            {
                _logger.LogError(
                    "Unhandled event type: {eventType}",
                    eventGridEvent.EventType);
            }

            _logger.LogInformation(
                "FUNCTION_END: " +
                "Event type: {eventType}, Event subject: {subject}",
                eventGridEvent.EventType, eventGridEvent.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "FUNCTION_EXCEPTION: {exceptionType} [{exceptionMessage}] " +
                "Event type: {eventType}, Event subject: {subject}",
                ex.GetType().FullName, ex.Message, eventGridEvent.EventType, eventGridEvent.Subject);
            throw;
        }
    }
}
