using TechAssistPro.SharedKernel.Events; 
using TechAssistPro.Ticketing.Entities; 
using TechAssistPro.Ticketing.Enums;


namespace TechAssistPro.Ticketing.Events;

public sealed record TicketStatusUpdatedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "ticket.status.updated";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;
    public TicketStatusUpdatedEventData Data { get; }

    public TicketStatusUpdatedDomainEvent(
        Guid ticketId,
        TicketStatus newStatus,
        string? comment,
        string updatedByUserId)
    {
        Data = new TicketStatusUpdatedEventData(
            ticketId,
            newStatus.ToString(), // Store as string for immutability and easier JSON serialization
            comment,
            updatedByUserId
        );
    }
}

public sealed record TicketStatusUpdatedEventData(
    Guid TicketId,
    string NewStatus, 
    string? Comment,
    string UpdatedByUserId
);