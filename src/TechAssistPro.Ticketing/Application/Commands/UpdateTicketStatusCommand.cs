using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace TechAssistPro.Ticketing.Application.Commands
{
   public record UpdateTicketStatusCommand(
    Guid TicketId,
    string NewStatus, 
    string? Comment) : IRequest<bool>;
}