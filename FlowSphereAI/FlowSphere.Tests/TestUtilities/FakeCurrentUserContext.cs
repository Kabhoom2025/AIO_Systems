using FlowSphere.Application.Common;

namespace FlowSphere.Tests.TestUtilities;

public class FakeCurrentUserContext : ICurrentUserContext
{
    public int OrganizationId { get; init; } = 1;
    public int UserId { get; init; } = 1;
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
