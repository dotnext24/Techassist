using TechAssistPro.SharedKernel.Events;

public sealed record TicketCreatedDomainEvent(Guid TicketId) : IDomainEvent;