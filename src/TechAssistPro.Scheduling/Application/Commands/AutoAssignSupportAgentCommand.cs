
using MediatR;
using TechAssistPro.Scheduling.Events;

namespace TechAssistPro.Scheduling.Application.Commands
{
    public sealed record AutoAssignSupportAgentCommand(
    Guid TicketId,
    string Category,
    string Priority) : IRequest<bool>;
}