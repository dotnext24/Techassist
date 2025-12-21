// Infrastructure/Events/DomainEventExtensions.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.Infrastructure.Messaging;
using TechAssistPro.SharedKernel.Domain;

namespace TechAssistPro.Infrastructure.Events;

public static class DomainEventExtensions
{
    public static async Task DispatchDomainEventsAsync(
        this DbContext context,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var entities = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            var notification =
                DomainEventNotificationFactory.Create(domainEvent);

            await mediator.Publish(notification, cancellationToken);
        }
    }
}