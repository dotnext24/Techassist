using Serilog.Core;
using Serilog.Events;
using TechAssistPro.Infrastructure.Observability;

namespace TechAssistPro.Scheduling.SerilogEnricher
{
    public sealed class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        var correlationId = CorrelationContext.CorrelationId;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            logEvent.AddPropertyIfAbsent(
                factory.CreateProperty("CorrelationId", correlationId));
        }
    }
}

}