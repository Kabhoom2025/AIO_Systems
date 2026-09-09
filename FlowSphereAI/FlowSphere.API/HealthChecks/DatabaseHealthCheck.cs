using FlowSphere.Infrastructure.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlowSphere.API.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FlowSphereDbContext _db;

    public DatabaseHealthCheck(FlowSphereDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _db.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Database connection OK.")
            : HealthCheckResult.Unhealthy("Cannot connect to the database.");
    }
}
