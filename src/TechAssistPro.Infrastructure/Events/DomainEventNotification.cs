using MediatR;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Infrastructure.Events
{
    public sealed class DomainEventNotification : INotification
    {
        public IDomainEvent DomainEvent { get; }

        public DomainEventNotification(IDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }

}