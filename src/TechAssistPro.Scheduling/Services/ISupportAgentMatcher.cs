using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechAssistPro.Scheduling.Entities;
using TechAssistPro.Scheduling.Events;

namespace TechAssistPro.Scheduling.Services
{
    public interface ISupportAgentMatcher
    {
        SupportAgent Match(IEnumerable<SupportAgent> agents, Guid ticketId, string category);
    }

}