using Microsoft.AspNetCore.Authorization;

namespace ProjectFlowAI.API.Authentication;

/// <summary>Mirrors NovaERP.Infrastructure.Authentication.PermissionRequirement — a distinct
/// implementation for this app since Program.cs is told not to reference NovaERP's assembly.</summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionKey { get; }

    public PermissionRequirement(string permissionKey) => PermissionKey = permissionKey;
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.PermissionKey))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
