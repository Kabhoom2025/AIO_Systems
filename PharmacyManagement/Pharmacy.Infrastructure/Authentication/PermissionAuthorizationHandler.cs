using Microsoft.AspNetCore.Authorization;

namespace Pharmacy.Infrastructure.Authentication;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissionsClaim = context.User.FindFirst("permissions")?.Value ?? string.Empty;
        var permissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (permissions.Contains(requirement.PermissionKey))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
