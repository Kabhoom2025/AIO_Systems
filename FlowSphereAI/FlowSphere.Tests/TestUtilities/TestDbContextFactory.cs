using FlowSphere.Application.Common;
using FlowSphere.Infrastructure.Data;
using FlowSphere.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FlowSphere.Tests.TestUtilities;

public static class TestDbContextFactory
{
    /// <summary>
    /// Matches production wiring exactly (see ServiceCollectionExtensions.AddFlowSphereInfrastructure) -
    /// replacing IModelCacheKeyFactory here too, so tests exercise the same per-tenant model
    /// caching behavior that fixed the "stale OrganizationId=0 baked into query filter" bug.
    /// </summary>
    public static FlowSphereDbContext Create(ICurrentUserContext currentUser, string? databaseName = null, ICurrentEnvironmentContext? currentEnvironment = null)
    {
        var options = new DbContextOptionsBuilder<FlowSphereDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>()
            .Options;

        return new FlowSphereDbContext(options, currentUser, currentEnvironment ?? new FakeCurrentEnvironmentContext());
    }
}
