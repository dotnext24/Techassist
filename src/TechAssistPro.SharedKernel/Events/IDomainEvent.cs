namespace TechAssistPro.SharedKernel.Events;
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAtUtc { get; }
    string EventType { get; }
    int SchemaVersion { get; }
}