using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Infrastructure.Events
{
    internal static class DomainEventNotificationFactory
    {
        public static DomainEventNotification Create(IDomainEvent domainEvent)
        => new(domainEvent);
    }
}