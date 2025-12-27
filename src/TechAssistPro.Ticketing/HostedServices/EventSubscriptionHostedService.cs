
using System.Diagnostics;
using Microsoft.Extensions.Options;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Ticketing.Events;

namespace TechAssistPro.Ticketing.HostedServices
{
   public sealed class EventSubscriptionHostedService : IHostedService
{
    private readonly RabbitMqEventSubscriber _subscriber;
    private readonly MessagingOptions _options;
    private readonly ActivitySource _activitySource;

    public EventSubscriptionHostedService(
        RabbitMqEventSubscriber subscriber,IOptions<MessagingOptions> options,ActivitySource activitySource)
    {
        _subscriber = subscriber;
        _options = options.Value;
        _activitySource=activitySource;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var ticketCreatedOptions=_options.Subscriptions["SupportAgentAssigned"];
         await _subscriber.SubscribeAsync<SupportAgentAssignedIntegrationEvent>(
            queueName: ticketCreatedOptions.QueueName,
            exchangeName:ticketCreatedOptions.ExchangeName,
            schemaVersion:ticketCreatedOptions.SchemaVersion,
            routingKeys: ticketCreatedOptions.RoutingKeys,
            ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

}