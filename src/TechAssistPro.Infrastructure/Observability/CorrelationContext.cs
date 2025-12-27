using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TechAssistPro.Infrastructure.Observability
{
  public static class CorrelationContext
    {
        private static readonly AsyncLocal<string?> _correlationId = new();

        public static string CorrelationId
        {
            get => _correlationId.Value ??= Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            set => _correlationId.Value = value;
        }
    }
}