using Microsoft.AspNetCore.Authorization;

namespace HRMS.Infrastructure.Authentication;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionKey { get; }

    public PermissionRequirement(string permissionKey) => PermissionKey = permissionKey;
}
