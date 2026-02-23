using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.TicketStates;

public abstract class TicketState
{
    public abstract TicketStatus Status { get; }

    public virtual void Assign(Ticket ticket, string agentId)
        => ThrowInvalid(nameof(Assign));

    public virtual void MarkInProgress(Ticket ticket, string comment, string userId)
        => ThrowInvalid(nameof(MarkInProgress));

    public virtual void Resolve(Ticket ticket, string comment, string userId)
        => ThrowInvalid(nameof(Resolve));

    public virtual void Reopen(Ticket ticket, string reason, string userId)
        => ThrowInvalid(nameof(Reopen));

    protected void ThrowInvalid(string action)
    {
        throw new DomainException(
            $"Action '{action}' not allowed in state '{Status}'.");
    }
}
