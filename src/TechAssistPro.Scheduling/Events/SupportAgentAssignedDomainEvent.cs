using TechAssistPro.Scheduling.Entities;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Scheduling.Events;

public sealed record SupportAgentAssignedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "support.agent.assigned";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;

    public SupportAgentAssignedEventData Data { get; init; } = default!;

    public SupportAgentAssignedDomainEvent(Assignment assignment)
    {
        Data = new SupportAgentAssignedEventData(assignment.TicketId, assignment.Id, assignment.SupportAgentId, assignment.CreatedAtUtc, assignment.UpdatedBy, assignment.LastUpdatedAtUtc);
    }
}

public sealed record SupportAgentAssignedEventData(
    Guid TicketId,
    Guid AssignmentId,
    Guid SupportAgentId,
    DateTime AssignedAtUtc,
    string? UpdatedBy,
    DateTime? LastUpdatedAtUtc
);
