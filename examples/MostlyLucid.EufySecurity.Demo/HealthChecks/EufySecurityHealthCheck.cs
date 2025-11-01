using Microsoft.Extensions.Diagnostics.HealthChecks;
using MostlyLucid.EufySecurity.Demo.Services;

namespace MostlyLucid.EufySecurity.Demo.HealthChecks;

/// <summary>
/// Health check for EufySecurity service
/// </summary>
public class EufySecurityHealthCheck(EufySecurityHostedService eufyService) : IHealthCheck
{
    private readonly EufySecurityHostedService _eufyService = eufyService;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_eufyService.IsConnected && _eufyService.Client != null)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("EufySecurity client is connected"));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy("EufySecurity client is not connected"));
    }
}
