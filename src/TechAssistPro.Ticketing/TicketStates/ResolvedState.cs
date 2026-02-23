using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.TicketStates;

public sealed class ResolvedState : TicketState
{
    public override TicketStatus Status => TicketStatus.Resolved;

    public override void Reopen(Ticket ticket, string reason, string userId)
    {
        ticket.SetState(new ReopenedState());
        ticket.AddDomainEvent(
            new TicketReopenedDomainEvent(ticket.Id, userId, reason));
    }
}
