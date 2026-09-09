using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Application.Common;

/// <summary>Shared by every handler/JWT-issuing code path that needs a user's effective roles/permissions.</summary>
public static class UserAuthorizationHelper
{
    public static async Task<(List<string> Roles, List<string> Permissions)> GetRolesAndPermissionsAsync(
        IProjectFlowDbContext db, Guid userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var roles = await db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        var permissions = await db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission!.Key)
            .Distinct()
            .ToListAsync(cancellationToken);

        return (roles, permissions);
    }
}
