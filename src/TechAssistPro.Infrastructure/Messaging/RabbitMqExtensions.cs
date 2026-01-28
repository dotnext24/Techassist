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
            return ReadHeaderAsString(headers, RabbitHeaders.CorrelationId)!;
        }

        public static string GetCorrelationId(
           this IDictionary<string, object> headers)
        {
            return ReadHeaderAsString(headers, RabbitHeaders.CorrelationId)!;

        }

      
          public static int GetRetryCount(
           this IDictionary<string, object> headers)
        {
            if (!headers.TryGetValue(RabbitHeaders.RetryCount, out var value))
                throw new InvalidOperationException($"{RabbitHeaders.RetryCount} header missing");

            return value switch
            {
                byte[] bytes => int.Parse(Encoding.UTF8.GetString(bytes)),
                int i => i,
                long l => (int)l,
                _ => throw new InvalidOperationException(
                    $"Unsupported {RabbitHeaders.RetryCount} header type: {value.GetType()}")
            };

        }

        private static string? ReadHeaderAsString(
        IDictionary<string, object>? headers,
        string key)
        {

            if (headers is null || !headers.TryGetValue(key, out var value))
                throw new InvalidOperationException($"{key} header missing");

            return value switch
            {
                byte[] bytes => Encoding.UTF8.GetString(bytes),
                string str => str,
                _ => value?.ToString()
            };
        }

    }

}