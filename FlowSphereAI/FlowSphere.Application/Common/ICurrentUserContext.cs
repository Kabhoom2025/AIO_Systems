namespace FlowSphere.Application.Common;

/// <summary>Resolves the current request's tenant/user from the JWT claims (organizationId,
/// sub, permissions) - the single source of truth used both by controllers and by the EF Core
/// global query filter in FlowSphereDbContext.</summary>
public interface ICurrentUserContext
{
    int OrganizationId { get; }
    int UserId { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool HasPermission(string permission);
}
