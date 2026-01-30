using System.Collections.Concurrent;

namespace TechAssistPro.Gateway.Middleware;

public class CircuitBreakerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CircuitBreakerMiddleware> _logger;
    private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers = new();

    public CircuitBreakerMiddleware(
        RequestDelegate next,
        ILogger<CircuitBreakerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var serviceName = GetServiceName(context.Request.Path);
        
        if (string.IsNullOrEmpty(serviceName))
        {
            await _next(context);
            return;
        }

        var circuitBreaker = _circuitBreakers.GetOrAdd(
            serviceName,
            _ => new CircuitBreakerState(serviceName, failureThreshold: 5, timeoutSeconds: 60));

        if (circuitBreaker.IsOpen)
        {
            _logger.LogWarning(
                "Circuit breaker OPEN for {ServiceName}. Request rejected",
                serviceName);

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Service Unavailable",
                message = $"The {serviceName} service is temporarily unavailable. Please try again later.",
                service = serviceName,
                retryAfter = circuitBreaker.GetRetryAfterSeconds()
            });
            return;
        }

        try
        {
            await _next(context);

            if (context.Response.StatusCode < 500)
            {
                circuitBreaker.RecordSuccess();
            }
            else
            {
                circuitBreaker.RecordFailure();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed for {ServiceName}", serviceName);
            circuitBreaker.RecordFailure();
            throw;
        }
    }

    private string? GetServiceName(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments == null || segments.Length < 2)
            return null;

        return segments[1] switch // After /api/
        {
            "tickets" => "ticketing",
            "technicians" or "assignments" => "scheduling",
            "customers" => "customer",
            "analytics" => "analytics",
            _ => null
        };
    }
}

public class CircuitBreakerState
{
    private readonly string _serviceName;
    private readonly int _failureThreshold;
    private readonly int _timeoutSeconds;
    private int _failureCount;
    private DateTime? _openedAt;

    public CircuitBreakerState(string serviceName, int failureThreshold, int timeoutSeconds)
    {
        _serviceName = serviceName;
        _failureThreshold = failureThreshold;
        _timeoutSeconds = timeoutSeconds;
    }

    public bool IsOpen
    {
        get
        {
            if (_openedAt.HasValue)
            {
                if (DateTime.UtcNow - _openedAt.Value > TimeSpan.FromSeconds(_timeoutSeconds))
                {
                    // Try half-open
                    _openedAt = null;
                    _failureCount = 0;
                    return false;
                }
                return true;
            }
            return false;
        }
    }

    public void RecordSuccess()
    {
        _failureCount = 0;
        _openedAt = null;
    }

    public void RecordFailure()
    {
        _failureCount++;
        
        if (_failureCount >= _failureThreshold)
        {
            _openedAt = DateTime.UtcNow;
        }
    }

    public int GetRetryAfterSeconds()
    {
        if (!_openedAt.HasValue)
            return 0;

        var elapsed = (DateTime.UtcNow - _openedAt.Value).TotalSeconds;
        return Math.Max(0, _timeoutSeconds - (int)elapsed);
    }
}