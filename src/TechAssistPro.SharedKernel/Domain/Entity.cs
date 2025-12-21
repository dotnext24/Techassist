using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.SharedKernel.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? LastUpdatedAtUtc { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected Entity() { }

    protected Entity(Guid id)
    {
        Id = id;
    }
}