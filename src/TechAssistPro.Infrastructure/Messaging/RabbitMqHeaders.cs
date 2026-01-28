using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.Messaging
{
    internal static class RabbitHeaders
    {
        public const string CorrelationId = "correlation.id";
        public const string CausationId = "causation.id";
        public const string Traceparent="traceparent";
        public const string SchemaVersion = "schema-version";
        public const string RetryCount = "x-retry-count";
    }

    internal static class DlqHeaders
    {
        public const string Reason = "x-dlq-reason";
        public const string ExceptionType = "x-dlq-exception-type";
        public const string FailedAtUtc = "x-dlq-failed-at";
        public const string SourceQueue = "x-dlq-source-queue";
    }


}