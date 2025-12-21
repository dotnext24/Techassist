
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Events;
using TechAssistPro.Scheduling.Exceptions;

namespace TechAssistPro.Scheduling.Services
{
    public sealed class SkillBasedSupportAgentMatcher
    : ISupportAgentMatcher
{
    public SupportAgent Match(IEnumerable<SupportAgent> agents, Guid ticketId, string category)
    {
        return agents
            .Where(a => a.CanHandle(category))
            .OrderBy(a => a.ActiveAssignments)
            .FirstOrDefault()
            ?? throw new NoSupportAgentAvailableException(ticketId);
    }
}

}