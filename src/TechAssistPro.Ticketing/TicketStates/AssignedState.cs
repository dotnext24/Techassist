using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.TicketStates;

public sealed class AssignedState : TicketState
{
    public override TicketStatus Status => TicketStatus.Assigned;

    public override void MarkInProgress(
        Ticket ticket,
        string comment,
        Guid actor)
    {
        ticket.TransitionTo(
            new InProgressState(),
            comment,
            actor);

        ticket.AddDomainEvent(
            new TicketMarkedInProgressDomainEvent(
                ticket.Id,
                actor,
                comment));
    }
}
