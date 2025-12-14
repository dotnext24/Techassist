using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.SharedKernel.Domain;
public abstract class Entity
{
    public Guid Id { get; protected set; }
     public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? LastUpdatedAtUtc { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    protected internal void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}