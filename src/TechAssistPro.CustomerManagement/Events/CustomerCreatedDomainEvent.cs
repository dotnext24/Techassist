using TechAssistPro.CustomerManagement.Entities;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.CustomerManagement.Events;

public sealed record CustomerCreatedDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public string EventType => "customer.created";
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public int SchemaVersion => 1;

    public CustomerCreatedEventData Data { get; init; } = default!;

    public CustomerCreatedDomainEvent(Customer customer)
    {
        Data = new CustomerCreatedEventData(customer.Id, customer.Name, customer.Email);
    }
}

public sealed record CustomerCreatedEventData(
    Guid CustomerId,
    string Name,
    string Email
);