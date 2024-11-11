// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridFunction

using System;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebhookFunctionApp.Functions;

/*
public class MyEventType
{
    public string Id { get; set; }

    public string Topic { get; set; }

    public string Subject { get; set; }

    public string EventType { get; set; }

    public DateTime EventTime { get; set; }

    public IDictionary<string, object> Data { get; set; }
}
*/

public class EventGridFunction
{
    private readonly ILogger<EventGridFunction> _logger;

    public EventGridFunction(ILogger<EventGridFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(EventGridFunction))]
    public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

        // Code from: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.eventgrid-readme?view=azure-dotnet#deserializing-event-data

        // If the event is a system event, TryGetSystemEventData will return the deserialized system event
        if (eventGridEvent.TryGetSystemEventData(out object systemEvent))
        {
            switch (systemEvent)
            {
                case SubscriptionValidationEventData subscriptionValidated:
                    Console.WriteLine(subscriptionValidated.ValidationCode);
                    break;
                case StorageBlobCreatedEventData blobCreated:
                    Console.WriteLine(blobCreated.BlobType);
                    break;
                // Handle any other system event type
                default:
                    Console.WriteLine(eventGridEvent.EventType);
                    // we can get the raw Json for the event using Data
                    Console.WriteLine(eventGridEvent.Data.ToString());
                    break;
            }
        }
        else
        {
            switch (eventGridEvent.EventType)
            {
                case "MyApp.Models.CustomEventType":
                    //TestPayload deserializedEventData = egEvent.Data.ToObjectFromJson<TestPayload>();
                    //Console.WriteLine(deserializedEventData.Name);
                    break;
                // Handle any other custom event type
                default:
                    Console.Write(eventGridEvent.EventType);
                    Console.WriteLine(eventGridEvent.Data.ToString());
                    break;
            }
        }
    }

    /* This didn't work
    [Function(nameof(EventGridFunction))]
    public void Run([EventGridTrigger] EventGridEvent[] eventGridEvents)
    {
        foreach (var eventGridEvent in eventGridEvents)
        {
            _logger.LogInformation("Event type: {type}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);
        }
    }
    */

    /* Basic CloudEvent
    [Function(nameof(EventGridFunction))]
    public void Run([EventGridTrigger] CloudEvent cloudEvent)
    {
        _logger.LogInformation("Event type: {type}, Event subject: {subject}", cloudEvent.Type, cloudEvent.Subject);

        _logger.LogInformation("Data content type: {contentType}, Data schema: {schema}", cloudEvent.DataContentType, cloudEvent.DataSchema);

        _logger.LogInformation("Data: {data}", cloudEvent.Data?.ToString());
    }
    */

    /* Custom type: MyEventType 
    [Function(nameof(EventGridFunction))]
    public void Run([EventGridTrigger] MyEventType myEvent)
    {
        _logger.LogInformation("Event subject: {subject}", myEvent.Subject);

        _logger.LogInformation("Data: {data}", JsonConvert.SerializeObject(myEvent.Data));
    }
    */
}
