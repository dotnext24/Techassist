using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.Scheduling.Events;

namespace TechAssistPro.Scheduling.HostedServices
{
   public sealed class EventSubscriptionHostedService : IHostedService
{
    private readonly RabbitMqEventSubscriber _subscriber;

    public EventSubscriptionHostedService(
        RabbitMqEventSubscriber subscriber)
    {
        _subscriber = subscriber;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _subscriber.SubscribeAsync<TicketCreatedIntegrationEvent>(
            queueName: "ticket.created.q",
            routingKeys: new[] { "ticket.created.v1" },
            ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

}