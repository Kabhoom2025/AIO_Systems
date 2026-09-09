using System.Linq.Expressions;
using FlowSphere.Application.Common;
using FlowSphere.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Persistence;

/// <summary>
/// Applies a global EF Core query filter to every entity implementing ITenantScoped
/// (OrganizationId == current tenant) and/or IEnvironmentScoped (Stage == current sandbox stage).
/// This is the explicit multi-tenancy/environment-isolation hardening this module adds over
/// sibling services in the repo, which resolve these manually per-controller with no
/// query-filter safety net. EF Core allows exactly one HasQueryFilter call per entity type, so an
/// entity implementing both interfaces gets a single combined (OrganizationId == X &amp;&amp;
/// Stage == Y) predicate rather than two separate filter calls.
/// </summary>
public static class OrganizationQueryFilterExtensions
{
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var isTenantScoped = typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType);
            var isEnvironmentScoped = typeof(IEnvironmentScoped).IsAssignableFrom(entityType.ClrType);

            if (!isTenantScoped && !isEnvironmentScoped)
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            Expression? filterBody = null;

            if (isTenantScoped)
            {
                var organizationIdProperty = Expression.Property(parameter, nameof(ITenantScoped.OrganizationId));
                var currentOrgId = Expression.Constant(currentUser.OrganizationId);
                filterBody = Expression.Equal(organizationIdProperty, currentOrgId);
            }

            if (isEnvironmentScoped)
            {
                var stageProperty = Expression.Property(parameter, nameof(IEnvironmentScoped.Stage));
                var currentStage = Expression.Constant(currentEnvironment.Stage);
                var stageFilter = Expression.Equal(stageProperty, currentStage);
                filterBody = filterBody is null ? stageFilter : Expression.AndAlso(filterBody, stageFilter);
            }

            var lambda = Expression.Lambda(filterBody!, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
