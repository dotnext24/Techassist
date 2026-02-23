using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Events;
using TechAssistPro.Ticketing.Enums;
using TechAssistPro.Ticketing.TicketStates;
using TechAssistPro.SharedKernel.Events;

namespace TechAssistPro.Ticketing.Entities;

public class Ticket : AggregateRoot
{
    private ITicketState _state= new NewState();    

    public string CustomerId { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    public TicketCategory Category { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketChannel Channel { get; private set; }
    public TicketStatus Status => _state.Status;
    public string? AssignedTechnicianId { get; private set; }

    public List<TicketHistory> History { get; private set; } = new();
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
        //ticket.AddDomainEvent(new TicketCreatedDomainEvent(this));

        return ticket;
    }


    public void ChangeStatus(
        TicketStatus targetStatus,
        string comment,
        Guid actor)
    {
        _state.TransitionTo(this, targetStatus, comment, actor);
    }

    internal void TransitionTo(
    TicketState newState,
    string comment,
    Guid actor)
{
    _state = newState;

    Touch(actor.ToString());
    AddHistory(newState.Status, comment, actor);
}



    public void Update(string subject, string description, TicketCategory category, TicketPriority priority, string updatedBy)
    {
        Subject = subject;
        Description = description;
        Category = category;
        Priority = priority;
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

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        RaiseDomainEvent(domainEvent);
    }

    private void AddHistory(
        TicketStatus status,
        string comment,
        Guid actor)
    {
        // persist history, raise events, etc.
    }

    internal void SetState(TicketState state)
    {
        _state = state;
        Touch(actor.ToString());
        RaiseDomainEvent(new TicketStatusUpdatedDomainEvent(this.Id,this._state.Status,comment,actor));       
        AddHistory(newState.Status, comment, actor);
    }

    // Public behaviors (NO LOGIC HERE)
    public void Assign(string agentId,string comment, string userId) {
        _state.Assign(this, agentId);
        Touch(userId);
        RaiseDomainEvent(new TicketStatusUpdatedDomainEvent(this.Id,this._state.Status,comment,userId));    
    }

    public void MarkInProgress(string agentId,string comment, string userId) {
        _state.MarkInProgress(this, agentId);
        Touch(userId);
        RaiseDomainEvent(new TicketStatusUpdatedDomainEvent(this.Id,this._state.Status,comment,userId));    
    }

    public void Resolve(string agentId,string comment, string userId) {
        _state.Resolve(this, agentId);
        Touch(userId);
        RaiseDomainEvent(new TicketStatusUpdatedDomainEvent(this.Id,this._state.Status,comment,userId));    
    }

    public void Reopen(string agentId,string comment, string userId) {
        _state.Reopen(this, agentId);
        Touch(userId);
        RaiseDomainEvent(new TicketStatusUpdatedDomainEvent(this.Id,this._state.Status,comment,userId));    
    }
}
