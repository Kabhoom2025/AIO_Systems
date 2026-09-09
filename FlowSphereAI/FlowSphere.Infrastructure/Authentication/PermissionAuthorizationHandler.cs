using Microsoft.AspNetCore.Authorization;

namespace FlowSphere.Infrastructure.Authentication;

/// <summary>
/// A foreign token (minted by a sibling service sharing the same JWT secret) that lacks the
/// required "permissions" claim value fails this handler with a 403, not a JWT validation
/// error - the intended, existing cross-service behavior in this repo (see plan Section 8).
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissionsClaim = context.User.FindFirst("permissions")?.Value ?? string.Empty;
        var permissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
