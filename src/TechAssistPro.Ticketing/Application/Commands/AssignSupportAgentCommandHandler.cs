using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TechAssistPro.Infrastructure.Observability;
using TechAssistPro.Ticketing.Data;
using TechAssistPro.Ticketing.Dtos;
using TechAssistPro.Ticketing.Events;
using TechAssistPro.Ticketing.Exceptions;

namespace TechAssistPro.Ticketing.Application.Commands
{
    public sealed class AssignSupportAgentCommandHandler
    : IRequestHandler<AssignSupportAgentCommand, bool>
    {
        private readonly ITicketRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignSupportAgentCommandHandler> _logger;
        private readonly ActivitySource _activitySource;
        public AssignSupportAgentCommandHandler(ITicketRepository repository, IMapper mapper, ILogger<AssignSupportAgentCommandHandler> logger, ActivitySource activitySource)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _activitySource = activitySource;

        }

        public async Task<bool> Handle(
            AssignSupportAgentCommand request,
            CancellationToken cancellationToken)
        {

            using var activity = _activitySource.StartActivity("Assign-Support-Agent");
            activity?.SetTag("ticket.id", request.TicketId);
            activity?.SetTag("support.agent.id", request.SupportAgentId);

            _logger.LogInformation("Assign-Support-Agent started | TicketId={TicketId}", request.TicketId);

            var ticket = await _repository.GetByIdAsync(request.TicketId, cancellationToken);

            if (ticket is null)
            {
                activity?.SetTag("idempotent.hit", true);
                throw new TicketNotFoundException(request.TicketId);
            }
            ticket.AssignSupportAgent(request.SupportAgentId!.ToString(), "System");

            await _repository.UpdateAsync(ticket, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return true;
        }
    }
}