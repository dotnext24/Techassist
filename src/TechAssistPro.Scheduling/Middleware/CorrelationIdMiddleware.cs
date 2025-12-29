using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TechAssistPro.Infrastructure.Observability;

namespace TechAssistPro.Scheduling.Middleware
{
    public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = CorrelationContext.CorrelationId;

        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
        Activity.Current?.SetTag("correlation.id", correlationId);

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path.Value ?? string.Empty,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception in request with correlation ID {CorrelationId}",
                    correlationId);
                throw;
            }
        }
    }
}
}