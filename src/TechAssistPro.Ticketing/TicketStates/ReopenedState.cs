using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.TicketStates;

public sealed class ReopenedState : ITicketState
{
    public TicketStatus Status => TicketStatus.Reopened;

    public void TransitionTo(
        Ticket ticket,
        TicketStatus target,
        string comment,
        Guid actorUserId)
    {
        switch (target)
        {
            case TicketStatus.Assigned: // Reopened tickets typically need re-assignment
            case TicketStatus.InProgress: // Could go directly to in-progress if same technician
                ticket.ApplyState(new InProgressState(), comment, actorUserId);
                break;
            case TicketStatus.Closed: // Reopened and then closed without work (e.g., duplicate)
                ticket.ApplyState(new ClosedState(), comment, actorUserId);
                break;
            default:
                throw InvalidTransition(target);
        }
    }

    private static DomainException InvalidTransition(TicketStatus target) =>
        new DomainException($"Cannot transition from '{TicketStatus.Reopened}' to '{target}'. Allowed: '{TicketStatus.Assigned}', '{TicketStatus.InProgress}', '{TicketStatus.Closed}'.");
}