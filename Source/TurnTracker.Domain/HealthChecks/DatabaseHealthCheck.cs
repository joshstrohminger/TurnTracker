using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TurnTracker.Data;

namespace TurnTracker.Domain.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly TurnContext _db;

    public DatabaseHealthCheck(TurnContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            if (await _db.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Healthy("Can connect to database");
            }

            return HealthCheckResult.Degraded("Cannot connect to database");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("Failed to connect to database", e);
        }
    }
}