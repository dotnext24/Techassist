using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TechAssistPro.Gateway.HealthChecks;

public class GatewayHealthCheck : IHealthCheck
{
    private readonly ILogger<GatewayHealthCheck> _logger;

    public GatewayHealthCheck(ILogger<GatewayHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check gateway health
            // You can add more sophisticated checks here
            
            return Task.FromResult(HealthCheckResult.Healthy("Gateway is healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Gateway is unhealthy", ex));
        }
    }
}
