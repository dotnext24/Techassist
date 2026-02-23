using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.TicketStates;

public sealed class PendingUserConfirmationState : ITicketState
{
    public TicketStatus Status => TicketStatus.PendingUserConfirmation;

    public void TransitionTo(
        Ticket ticket,
        TicketStatus target,
        string comment,
        Guid actorUserId)
    {
        switch (target)
        {
            case TicketStatus.Closed: // Customer confirms or auto-closes
                ticket.ApplyState(new ClosedState(), comment, actorUserId);
                break;
            case TicketStatus.Reopened: // Customer disputes resolution
                ticket.ApplyState(new ReopenedState(), comment, actorUserId);
                break;
            default:
                throw InvalidTransition(target);
        }
    }

    private static DomainException InvalidTransition(TicketStatus target) =>
        new DomainException($"Cannot transition from '{TicketStatus.PendingUserConfirmation}' to '{target}'. Allowed: '{TicketStatus.Closed}', '{TicketStatus.Reopened}'.");
}