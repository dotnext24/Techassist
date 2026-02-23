using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.TicketStates;

public sealed class InProgressState : TicketState
{
    public override TicketStatus Status => TicketStatus.InProgress;

    public override void Resolve(Ticket ticket, string comment, string userId)
    {
        ticket.SetState(new ResolvedState());
        // ticket.AddDomainEvent(
        //     new TicketResolvedDomainEvent(ticket.Id, userId, comment));
    }
}
