using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Scheduling.Entities
{
    public sealed record AgentAvailability(bool IsAvailable)
{
    public static AgentAvailability Available()
        => new(true);

    public static AgentAvailability Unavailable()
        => new(false);
}

}