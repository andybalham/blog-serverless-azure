using System.Text.Json;
using Azure.Messaging.EventGrid;

namespace WebhookFunctionApp.Utilities;

public static class EventGridExtensions
{
    public static string GetDataAsJson(this EventGridEvent eventGridEvent)
    {
        if (eventGridEvent == null)
            throw new ArgumentNullException(nameof(eventGridEvent));

        // Convert BinaryData to string first
        string dataString = eventGridEvent.Data.ToString();

        try
        {
            // Verify if it's valid JSON
            using (JsonDocument.Parse(dataString))
            {
                return dataString;
            }
        }
        catch (JsonException)
        {
            // If not valid JSON, serialize it as a plain string
            return JsonSerializer.Serialize(dataString);
        }
    }
}
