using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.Messaging
{
   public static class RabbitMqHeaderExtensions
{
    public static int GetSchemaVersion(
        this IDictionary<string, object> headers)
    {
        if (!headers.TryGetValue("schema-version", out var value))
            throw new InvalidOperationException("Schema version header missing");

        return value switch
        {
            byte[] bytes => int.Parse(Encoding.UTF8.GetString(bytes)),
            int i => i,
            long l => (int)l,
            _ => throw new InvalidOperationException(
                $"Unsupported schema version header type: {value.GetType()}")
        };
    }

     public static string GetTraceParent(
        this IDictionary<string, object> headers)
    {
        if (!headers.TryGetValue("trace-parent", out var value))
            throw new InvalidOperationException("Trace parent header missing");

        return value.ToString()!;
    }

     public static string GetCorrelationId(
        this IDictionary<string, object> headers)
    {
        if (!headers.TryGetValue("correlation-id", out var value))
            throw new InvalidOperationException("Correlation id header missing");

        return value.ToString()!;
    }
}

}