
using AutoMapper;
using MediatR;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Services;
using TechAssistPro.Scheduling.Data;
using System.Diagnostics;
using TechAssistPro.Infrastructure.Observability;


namespace TechAssistPro.Scheduling.Application.Commands
{
    public sealed class AutoAssignSupportAgentCommandHandler
    : IRequestHandler<AutoAssignSupportAgentCommand, bool>
    {
        private readonly ISupportAgentRepository _agentRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ISupportAgentMatcher _matcher;
        private readonly IMapper _mapper;
        private readonly ILogger<AutoAssignSupportAgentCommandHandler> _logger;
        private readonly ActivitySource _activitySource;
        public AutoAssignSupportAgentCommandHandler(ISupportAgentRepository agentRepository, IAssignmentRepository assignmentRepository, ISupportAgentMatcher matcher, IMapper mapper, ILogger<AutoAssignSupportAgentCommandHandler> logger, ActivitySource activitySource)
        {
            _agentRepository = agentRepository;
            _assignmentRepository = assignmentRepository;
            _matcher = matcher;
            _mapper = mapper;
            _logger = logger;
            _activitySource = activitySource;
        }

        public async Task<bool> Handle(
            AutoAssignSupportAgentCommand command,
            CancellationToken ct)
        {

            using var activity = _activitySource.StartActivity("Auto-Assign-Support-Agent");
            activity?.SetTag("ticket.id", command.TicketId);
            activity?.SetTag("ticket.category", command.Category);
            activity?.SetTag("correlation.id", CorrelationContext.CorrelationId);

            _logger.LogInformation("Auto-Assign-Support-Agent started | TicketId={TicketId} | Category={Category}", command.TicketId, command.Category);


            try
            {
                // 🛑 Idempotency check
                var existing = await _assignmentRepository
                    .GetByTicketIdAsync(command.TicketId, ct);

                if (existing is not null)
                {
                    _logger.LogInformation("Auto-Assign-Support-Agent - Already assigned - Existing assignment for same ticket: AssignmentId {assignmentId}", existing.Id);
                    return true; // already processed
                }

                var agents = await _agentRepository.GetAvailableAsync(ct);
                _logger.LogInformation("Auto-Assign-Support-Agent - Agents Available: {count}", agents.Count());


                var agent = _matcher.Match(agents, command.TicketId, command.Category);
                _logger.LogInformation("Auto-Assign-Support-Agent - Agent Matched: AgentId {Id}", agent.Id);

                var assignment = Assignment.Create(
                command.TicketId,
                agent.Id, null);

                agent.Assign();

                await _assignmentRepository.AddAsync(assignment, ct);
                await _agentRepository.UpdateAsync(agent, ct);

                activity?.SetTag("agent.id", agent.Id);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation("Auto-Assign-Support-Agent succeeded | AgentId={AgentId}", agent.Id);

                return true;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(
                    ex,
                    "Error in Auto-Assign-Support-Agent command handler | TicketId {TicketId}",
                    command.TicketId);

                throw;
            }
        }
    }
}