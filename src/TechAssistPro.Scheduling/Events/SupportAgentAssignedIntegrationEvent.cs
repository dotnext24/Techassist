namespace TechAssistPro.Scheduling.Events;
public sealed record SupportAgentAssignedIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "support.agent.assigned";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;
     
    public SupportAgentAssignedEventData Data { get; init; } = default!;
}

