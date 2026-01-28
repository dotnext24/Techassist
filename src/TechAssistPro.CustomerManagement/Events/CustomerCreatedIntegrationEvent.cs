using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.CustomerManagement.Events;

public sealed record CustomerCreatedIntegrationEvent 
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "customer.created";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;

    public CustomerCreatedEventData Data { get; init; } = default!;

    public CustomerCreatedIntegrationEvent(CustomerCreatedEventData data)
    {
        Data = data;
    }
}

