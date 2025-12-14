
using MediatR;
using TechAssistPro.Ticketing.Dtos;

namespace TechAssistPro.Ticketing.Application.Commands
{
    public sealed record CreateTicketCommand(
    string CustomerId,
    string Subject,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    TicketChannel Channel,
    string CreatedBy
) : IRequest<TicketResponseDto>;
}