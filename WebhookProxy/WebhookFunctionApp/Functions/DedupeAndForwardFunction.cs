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

namespace WebhookFunctionApp.Functions;

public class DedupeAndForwardFunction
{
    private readonly ILogger<DedupeAndForwardFunction> _logger;

    public DedupeAndForwardFunction(ILogger<DedupeAndForwardFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(DedupeAndForwardFunction))]
    public void Run([EventGridTrigger] CloudEvent cloudEvent) // TODO: Mention this is the default
    {
        _logger.LogInformation("DedupeAndForwardFunction Version: 241117-1025");

        _logger.LogInformation(
            "Event type: {type}, Event subject: {subject}", 
            cloudEvent.Type, cloudEvent.Subject);

        _logger.LogInformation(
            "Event: {event}", 
            JsonSerializer.Serialize(cloudEvent, options: new() { WriteIndented = true }));

        // Code from: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.eventgrid-readme?view=azure-dotnet#deserializing-event-data

        // If the event is a system event, TryGetSystemEventData will return the deserialized system event
        if (cloudEvent.TryGetSystemEventData(out object systemEvent))
        {
            switch (systemEvent)
            {
                case SubscriptionValidationEventData subscriptionValidated:
                    Console.WriteLine(subscriptionValidated.ValidationCode);
                    break;
                case StorageBlobCreatedEventData blobCreated:
                    _logger.LogInformation("blobCreated.Url: {url}", blobCreated.Url);
                    //var acceptedPayload = await _payloadStore.GetAcceptedPayloadAsync(blobCreated.Url);
                    //_logger.LogInformation("acceptedPayload: {acceptedPayload}", JsonSerializer.Serialize(acceptedPayload));
                    break;
                // Handle any other system event type
                default:
                    // TODO: Log an error
                    Console.WriteLine(cloudEvent.Type);
                    break;
            }
        }
        else
        {
            // TODO: Log an error
            Console.Write(cloudEvent.Type);
        }
    }
}
