using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Scheduling.Events;
public sealed record TicketCreatedIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "ticket.created";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;
     
   public TicketCreatedEventData Data { get; init; } = default!;
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
