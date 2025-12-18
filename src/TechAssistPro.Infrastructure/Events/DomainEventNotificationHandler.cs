using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Infrastructure.Events;

public sealed class DomainEventNotificationHandler
    : INotificationHandler<DomainEventNotification>
{
    private readonly IServiceProvider _provider;

    public DomainEventNotificationHandler(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        Console.WriteLine("🔥 DOMAIN EVENT HANDLER HIT");

        var domainEvent = notification.DomainEvent;

        // Resolve all event-specific handlers dynamically
        var handlerType = typeof(IEventHandler<>)
            .MakeGenericType(domainEvent.GetType());

        var handlers = _provider.GetServices(handlerType);

        foreach (dynamic handler in handlers)
        {
            if (handler != null)
                await handler.Handle((dynamic)domainEvent, cancellationToken);
        }
    }
}
