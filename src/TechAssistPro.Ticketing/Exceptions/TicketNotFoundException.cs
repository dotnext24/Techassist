// Ticketing/Application/Tickets/Exceptions/TicketNotFoundException.cs
using TechAssistPro.SharedKernel.Exceptions;

namespace TechAssistPro.Ticketing.Exceptions;

public sealed class TicketNotFoundException : NotFoundException
{
    public TicketNotFoundException(Guid ticketId)
        : base($"Ticket '{ticketId}' was not found.")
    {
    }
}
