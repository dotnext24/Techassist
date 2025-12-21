// Ticketing/Application/Tickets/Exceptions/TicketNotFoundException.cs
using TechAssistPro.SharedKernel.Exceptions;
namespace TechAssistPro.Scheduling.Exceptions;

public sealed class NoSupportAgentAvailableException : NotFoundException
{
    public NoSupportAgentAvailableException(Guid ticketId)
        : base($"SupportAgent is not available for new assignment. Ticket '{ticketId}'")
    {
    }
}
