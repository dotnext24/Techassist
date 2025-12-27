using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.Messaging
{
    public sealed class MessagingOptions
    {
        public string ExchangeName { get; init; } = default!;
        public Dictionary<string, SubscriptionDetail> Subscriptions { get; init; }
        = new();
    }

    public sealed class SubscriptionDetail
    {
        public string ExchangeName { get; init; } = default!;
        public string QueueName { get; init; } = default!;
        public int SchemaVersion { get; init; } = default!;
        public string[] RoutingKeys { get; init; } = Array.Empty<string>();
    }
}