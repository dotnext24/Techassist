// Infrastructure/Events/DomainEventExtensions.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechAssistPro.SharedKernel.Domain;

public static class DomainEventExtensions
{
    public static async Task DispatchDomainEventsAsync(
        this IMediator mediator,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
