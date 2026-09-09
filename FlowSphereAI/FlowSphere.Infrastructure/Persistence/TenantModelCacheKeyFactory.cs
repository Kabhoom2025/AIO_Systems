using FlowSphere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FlowSphere.Infrastructure.Persistence;

/// <summary>
/// EF Core caches the compiled model per DbContext type by default, which would freeze the
/// OrganizationId/Stage global query filters (built in OnModelCreating) to whatever tenant/stage
/// happened to build the model FIRST - every later request's filter would silently compare
/// against that stale value instead of the current caller's. Keying the model cache on both the
/// current tenant id and the current environment stage forces a distinct compiled model per
/// (organization, stage) pair so both filters are always live.
/// </summary>
public class TenantModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(Microsoft.EntityFrameworkCore.DbContext context, bool designTime)
    {
        if (context is FlowSphereDbContext flowSphereContext)
        {
            return (context.GetType(), flowSphereContext.CurrentOrganizationId, flowSphereContext.CurrentEnvironmentStage, designTime);
        }

        return (context.GetType(), 0, designTime);
    }
}
