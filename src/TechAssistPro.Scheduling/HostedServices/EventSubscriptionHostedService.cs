using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Scheduling.Events;

namespace TechAssistPro.Scheduling.HostedServices
{
   public sealed class EventSubscriptionHostedService : IHostedService
{
    private readonly RabbitMqEventSubscriber _subscriber;
    private readonly MessagingOptions _options;

    public EventSubscriptionHostedService(
        RabbitMqEventSubscriber subscriber,IOptions<MessagingOptions> options)
    {
        _subscriber = subscriber;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var ticketCreatedOptions=_options.Subscriptions["TicketCreated"];
         await _subscriber.SubscribeAsync<TicketCreatedIntegrationEvent>(
            queueName: ticketCreatedOptions.QueueName,
            exchangeName:ticketCreatedOptions.ExchangeName,
            schemaVersion:ticketCreatedOptions.SchemaVersion,
            routingKeys: ticketCreatedOptions.RoutingKeys,
            ct);
       
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

}