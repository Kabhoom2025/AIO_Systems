using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Common;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ChangeUserRoleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var target = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (target is null)
        {
            return Result.Failure(Error.NotFound("User not found."));
        }

        if (target.Id == _currentUser.UserId)
        {
            return Result.Failure(Error.Conflict("You cannot change your own role."));
        }

        if (target.Role?.Name == request.RoleName)
        {
            return Result.Success();
        }

        if (target.Role?.Name == "Admin" && request.RoleName != "Admin")
        {
            var activeAdminCount = await _db.Users
                .CountAsync(u => u.RoleId == target.RoleId && u.IsActive, cancellationToken);

            if (activeAdminCount <= 1)
            {
                return Result.Failure(Error.Conflict("Cannot change the role of the last admin in the organization."));
            }
        }

        var newRole = await _db.Roles
            .FirstOrDefaultAsync(r => r.OrganizationId == _currentUser.OrganizationId && r.Name == request.RoleName, cancellationToken);

        if (newRole is null)
        {
            newRole = new Role
            {
                OrganizationId = _currentUser.OrganizationId,
                Name = request.RoleName,
                Permissions = string.Join(',', PermissionCatalog.PermissionsForRoleName(request.RoleName)),
            };
            _db.Roles.Add(newRole);
            await _db.SaveChangesAsync(cancellationToken);
        }

        target.RoleId = newRole.Id;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
