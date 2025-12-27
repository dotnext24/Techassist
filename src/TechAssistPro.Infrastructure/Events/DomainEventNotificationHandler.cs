using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Infrastructure.Events;

public sealed class DomainEventNotificationHandler
    : INotificationHandler<DomainEventNotification>
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<DomainEventNotificationHandler> _logger;
    public DomainEventNotificationHandler(IServiceProvider provider, ILogger<DomainEventNotificationHandler> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken)
    {

        var domainEvent = notification.DomainEvent;

        _logger.LogDebug("EventNotificationHandler is called for {EventTYpe}", domainEvent.GetType());

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
