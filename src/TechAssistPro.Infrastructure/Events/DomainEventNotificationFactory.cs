using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Infrastructure.Events
{
    internal static class DomainEventNotificationFactory
{
    public static INotification Create(IDomainEvent domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>)
            .MakeGenericType(domainEvent.GetType());

        var notification = Activator.CreateInstance(
            notificationType,
            domainEvent);

        return notification as INotification
            ?? throw new InvalidOperationException(
                $"Failed to create INotification for domain event {domainEvent.GetType().Name}");
    }
}
}