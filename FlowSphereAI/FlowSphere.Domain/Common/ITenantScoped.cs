namespace FlowSphere.Domain.Common;

/// <summary>
/// Marker for entities that must be filtered by OrganizationId. Every implementer gets an
/// automatic EF Core global query filter applied in FlowSphereDbContext.OnModelCreating -
/// this is the multi-tenancy hardening that sibling services in this repo do not yet have.
/// </summary>
public interface ITenantScoped
{
    int OrganizationId { get; set; }
}
