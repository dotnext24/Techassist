using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.SharedKernel.Events
{
    public interface IEventHandler
    {
        string EventType { get; }
        int[] SupportedVersions { get; }
        Task HandleAsync(string jsonPayload, int version, CancellationToken cancellationToken = default);
    }
}