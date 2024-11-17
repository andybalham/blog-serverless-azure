// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridFunction

using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebhookFunctionApp.Services.PayloadStore;

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
    private readonly IPayloadStore _payloadStore;

    public EventGridFunction(ILogger<EventGridFunction> logger, IPayloadStore payloadStore)
    {
        _logger = logger;
        _payloadStore = payloadStore;
    }

    //[Function(nameof(EventGridFunction))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("EventGridFunction Version: 241116-1557");

        // Convert entire event to JSON

        string eventNewtonsoftJson = 
            Newtonsoft.Json.JsonConvert.SerializeObject(
                eventGridEvent, Newtonsoft.Json.Formatting.Indented);
        _logger.LogInformation($"Full event:\n{eventNewtonsoftJson}");

        string eventSystemTextJson = 
            System.Text.Json.JsonSerializer.Serialize(
                eventGridEvent, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
        _logger.LogInformation($"Full event:\n{eventSystemTextJson}");

        var eventJsonNewtonsoft = """
             {
                "Data": {
                },
                "DataVersion": "",
                "EventTime": "2024-11-12T18:17:38.4562975+00:00",
                "EventType": "Microsoft.Storage.BlobCreated",
                "Id": "9285afe2-b01e-002c-562f-356ccf0608c2",
                "Subject": "/blobServices/default/containers/webhook-payloads-accepted/blobs/LovelyLoans/QuickValuationCo/2024-11-12/2024-11-12T18:17:37UTC-73c4470b-b629-4fdf-b9ea-d9dd4332143f.json",
                "Topic": "/subscriptions/37645d0d-da4b-46a8-aa99-dbefe2e1185e/resourceGroups/WebhookProxy-ClickOps/providers/Microsoft.Storage/storageAccounts/webhookstorageclickops"
            }
            """;

        var blobCreatedJson = """
                        {
                "Api": "PutBlob",
                "BlobType": "BlockBlob",
                "ClientRequestId": "87587374-4264-4e5c-875a-5d912c31465e",
                "ContentLength": 2130,
                "ContentOffset": null,
                "ContentType": "application/octet-stream",
                "ETag": "0x8DD034721B5280E",
                "Identity": null,
                "RequestId": "fc9bee32-b01e-003c-122f-35a9a7000000",
                "Sequencer": "0000000000000000000000000003C5ED00000000000f3e74",
                "StorageDiagnostics": {
                    "batchId": "737bfd88-4006-005a-002f-35e687000000"
                },
                "Url": "https://webhookstorageclickops.blob.core.windows.net/webhook-payloads-accepted/LovelyLoans/QuickValuationCo/2024-11-12/2024-11-12T18:23:39UTC-e57ddd5d-5f47-4621-83e5-b7997766c573.json"
            }
            """;

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
                    // Oddly, System.Text.Json throws an exception serializing this object
                    /*
                     Exception: System.NotImplementedException: The method or operation is not implemented.
                       at Azure.Messaging.EventGrid.SystemEvents.StorageBlobCreatedEventData.StorageBlobCreatedEventDataConverter.Write(Utf8JsonWriter writer, StorageBlobCreatedEventData model, JsonSerializerOptions options)
                       ...
                       at System.Text.Json.JsonSerializer.Serialize[TValue](TValue value, JsonSerializerOptions options)
                     */
                    _logger.LogInformation("blobCreated: {blobCreated}", Newtonsoft.Json.JsonConvert.SerializeObject(blobCreated));
                    var acceptedPayload = await _payloadStore.GetAcceptedPayloadAsync(blobCreated.Url);
                    _logger.LogInformation("acceptedPayload: {acceptedPayload}", System.Text.Json.JsonSerializer.Serialize(acceptedPayload));
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
