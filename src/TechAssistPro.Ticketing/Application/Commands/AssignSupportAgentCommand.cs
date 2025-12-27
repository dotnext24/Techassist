
using MediatR;
using TechAssistPro.Ticketing.Dtos;

namespace TechAssistPro.Ticketing.Application.Commands
{
    public sealed record AssignSupportAgentCommand(
    Guid TicketId,
    Guid AssignmentId,
    Guid SupportAgentId
) : IRequest<bool>;
}