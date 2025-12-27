using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Events;

public class Ticket : AggregateRoot
{

    public string CustomerId { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    public TicketCategory Category { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketChannel Channel { get; private set; }
    public TicketStatus Status { get; private set; }

    public string? AssignedTechnicianId { get; private set; }


    private Ticket() { } // EF Core uses this

    public Ticket(
        Guid id,
        string customerId,
        string subject,
        string description,
        TicketCategory category,
        TicketPriority priority,
        TicketChannel channel)
    {
        Id = id;
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        Subject = !string.IsNullOrWhiteSpace(subject)
                    ? subject
                    : throw new ArgumentException("Subject is required");

        Description = description ?? string.Empty;

        Category = category;
        Priority = priority;
        Channel = channel;

        Status = TicketStatus.New;

        CreatedAtUtc = DateTime.UtcNow;
    }

    public static Ticket Create(
    string customerId,
    string subject,
    string description,
    TicketCategory category,
    TicketPriority priority,
    TicketChannel channel,
    string createdBy)
    {
        Guid id = Guid.NewGuid();
        var ticket = new Ticket(
            id,
            customerId,
            subject,
            description,
            category,
            priority,
            channel);
        ticket.Touch(createdBy);
        ticket.AddCreatedEvent();

        return ticket;
    }


    public void Update(string subject, string description, TicketCategory category, TicketPriority priority, string updatedBy)
    {
        Subject = subject;
        Description = description;
        Category = category;
        Priority = priority;
        Touch(updatedBy);
    }

    public void ChangeStatus(TicketStatus status, string updatedBy)
    {
        Status = status;
        Touch(updatedBy);
    }

    public void AssignSupportAgent(string technicianId, string updatedBy)
    {
        AssignedTechnicianId = technicianId;
        Touch(updatedBy);
    }

    public void SoftDelete(string deletedBy)
    {
        IsDeleted = true;
        Touch(deletedBy);
    }

    private void Touch(string updatedBy)
    {
        LastUpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    private void AddCreatedEvent()
    {
        RaiseDomainEvent(new TicketCreatedDomainEvent(this));
    }
}
