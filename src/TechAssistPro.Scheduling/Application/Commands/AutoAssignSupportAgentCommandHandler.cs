
using AutoMapper;
using MediatR;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Services;
using TechAssistPro.Ticketing.Data;


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
        public AutoAssignSupportAgentCommandHandler(ISupportAgentRepository agentRepository, IAssignmentRepository assignmentRepository, ISupportAgentMatcher matcher, IMapper mapper, ILogger<AutoAssignSupportAgentCommandHandler> logger)
        {
            _agentRepository = agentRepository;
            _assignmentRepository = assignmentRepository;
            _matcher = matcher;
            _mapper = mapper;
            _logger = logger;

        }

        public async Task<bool> Handle(
            AutoAssignSupportAgentCommand command,
            CancellationToken ct)
        {
            _logger.LogInformation("AutoAssignSupportAgentCommandHandler Called");

            // 🛑 Idempotency check
            var existing = await _assignmentRepository
                .GetByTicketIdAsync(command.TicketId, ct);

            if (existing is not null)
            {
                _logger.LogInformation("AutoAssignSupportAgentCommandHandler existing assignment for same ticket: {assignmentId}", existing.Id);
                return true; // already processed
            }

            var agents = await _agentRepository.GetAvailableAsync(ct);
            _logger.LogInformation("AutoAssignSupportAgentCommandHandler agents Available: {count}", agents.Count());


            var agent = _matcher.Match(agents, command.TicketId, command.Category);
            _logger.LogInformation("AutoAssignSupportAgentCommandHandler agent Matched: {Id}", agent.Id);

            var assignment = Assignment.Create(
            command.TicketId,
            agent.Id, null);

            agent.Assign();

            await _assignmentRepository.AddAsync(assignment, ct);
            await _agentRepository.UpdateAsync(agent, ct);

            //publish event


            return true;
        }
    }
}