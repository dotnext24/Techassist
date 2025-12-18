using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.SharedKernel.Events
{
    public interface IIntegrationEventHandler<TEvent>
    {
        Task HandleAsync(TEvent @event, CancellationToken ct);
    }

}