using TechAssistPro.Ticketing.Enums;
using TechAssistPro.SharedKernel.Domain;
using TechAssistPro.Ticketing.Entities;
using TechAssistPro.SharedKernel.Exceptions;
using TechAssistPro.Ticketing.Events;

namespace TechAssistPro.Ticketing.TicketStates;

public sealed class NewState : TicketState
{
    public override TicketStatus Status => TicketStatus.New;

    public override void Assign(Ticket ticket, string agentId)
    {
        ticket.SetState(new AssignedState());        
    }
}