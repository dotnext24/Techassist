using System.Diagnostics;

namespace TechAssistPro.Infrastructure.Observability;

public static class Observability
{
    public static ActivitySource CreateActivitySource(string serviceName)
        => new(serviceName);

    public static string GetCorrelationId()
        => Activity.Current?.TraceId.ToString()
           ?? Guid.NewGuid().ToString();
}
