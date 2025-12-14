using TechAssistPro.SharedKernel.Events;

public sealed record TicketCreatedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "ticket.created";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;

    public TicketCreatedEventData Data { get; }
    public TicketCreatedDomainEvent(Ticket ticket)
    {
        Data = new TicketCreatedEventData(
            ticket.Id,
            ticket.CustomerId,
            ticket.Subject,
            ticket.Description,
            ticket.Category.ToString(),
            ticket.Priority.ToString(),
            ticket.Channel.ToString(),
            ticket.Status.ToString(),
            ticket.AssignedTechnicianId,
            ticket.CreatedAtUtc,
            ticket.UpdatedBy,
            ticket.LastUpdatedAtUtc
        );
    }
}

public sealed record TicketCreatedEventData(
    Guid TicketId,
    string CustomerId,
    string Subject,
    string Description,
    string Category,
    string Priority,
    string Channel,
    string Status,
    string? AssignedTechnicianId,
    DateTime CreatedAtUtc,
    string? UpdatedBy,
    DateTime? LastUpdatedAtUtc
);
