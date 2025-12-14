using MediatR;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Infrastructure.Events
{
   public sealed record DomainEventNotification<TDomainEvent>(
    TDomainEvent DomainEvent
) : INotification
    where TDomainEvent : IDomainEvent;
}