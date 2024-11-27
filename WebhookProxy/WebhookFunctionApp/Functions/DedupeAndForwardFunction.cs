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
using WebhookFunctionApp.Models;
using WebhookFunctionApp.Services.EndpointProxy;

namespace WebhookFunctionApp.Functions;

public class DedupeAndForwardFunction
{
    private readonly ILogger<DedupeAndForwardFunction> _logger;
    private readonly IPayloadStore _payloadStore;
    private readonly IEndpointProxyFactory _endpointProxyFactory;

    public DedupeAndForwardFunction(
        ILogger<DedupeAndForwardFunction> logger,
        IPayloadStore payloadStore,
        IEndpointProxyFactory endpointProxyFactory)
    {
        _logger = logger;
        _payloadStore = payloadStore;
        _endpointProxyFactory = endpointProxyFactory;
    }

    [Function(nameof(DedupeAndForwardFunction))]
    public async Task RunAsync([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogDebug("{FunctionName} Version: 241124-1146", nameof(DedupeAndForwardFunction));

        try
        {
            _logger.LogInformation(
                "FUNCTION_START: " +
                "Event type: {eventType}, Event subject: {subject}",
                eventGridEvent.EventType, eventGridEvent.Subject);

            _logger.LogDebug(
                "EVENT: {event}",
                JsonSerializer.Serialize(eventGridEvent));

            _logger.LogDebug("DATA: {data}", eventGridEvent.Data.ToString());

            await RunInternalAsync(eventGridEvent);

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
            // Don't throw be default, as this retries
            //throw;
        }
    }

    private async Task RunInternalAsync(EventGridEvent eventGridEvent)
    {
        // Code from: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.eventgrid-readme?view=azure-dotnet#deserializing-event-data

        if (eventGridEvent.TryGetSystemEventData(out object systemEvent))
        {
            switch (systemEvent)
            {
                case SubscriptionValidationEventData subscriptionValidated:
                    _logger.LogInformation(
                        "subscriptionValidated.ValidationCode: {code}",
                        subscriptionValidated.ValidationCode);
                    break;

                case StorageBlobCreatedEventData blobCreated:
                    _logger.LogDebug(
                        "blobCreated.Url: {url}",
                        blobCreated.Url);

                    var acceptedPayload = 
                        await _payloadStore.GetAcceptedPayloadAsync(blobCreated.Url);

                    _logger.LogInformation(
                        "acceptedPayload: {acceptedPayload}", 
                        JsonSerializer.Serialize(acceptedPayload));

                    var endpointProxy = 
                        _endpointProxyFactory.GetEndpointProxy(
                            acceptedPayload.TenantId, acceptedPayload.ContractId);

                    await endpointProxy.InvokeAsync(acceptedPayload.Body);

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
    }
}
