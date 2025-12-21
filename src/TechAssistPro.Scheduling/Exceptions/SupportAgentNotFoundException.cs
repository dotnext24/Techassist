using TechAssistPro.SharedKernel.Exceptions;
namespace TechAssistPro.Scheduling.Exceptions;

public sealed class SupportAgentNotFoundException : NotFoundException
{
    public SupportAgentNotFoundException(Guid agentId)
        : base($"Agent '{agentId}' was not found.")
    {
    }
}
