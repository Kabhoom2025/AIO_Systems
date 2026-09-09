using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Common;

/// <summary>
/// Marker for entities that must be filtered by EnvironmentStage (Dev/QA/UAT/Live). Every
/// implementer gets an automatic EF Core global query filter applied in
/// FlowSphereDbContext.OnModelCreating, combined with the ITenantScoped filter when an entity
/// implements both - see OrganizationQueryFilterExtensions.ApplyGlobalFilters.
/// </summary>
public interface IEnvironmentScoped
{
    EnvironmentStage Stage { get; set; }
}
