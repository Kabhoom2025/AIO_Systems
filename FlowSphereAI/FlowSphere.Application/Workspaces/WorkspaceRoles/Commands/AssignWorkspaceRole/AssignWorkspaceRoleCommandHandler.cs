using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.AssignWorkspaceRole;

public class AssignWorkspaceRoleCommandHandler : IRequestHandler<AssignWorkspaceRoleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public AssignWorkspaceRoleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AssignWorkspaceRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _db.WorkspaceRoles.FirstOrDefaultAsync(
            r => r.Id == request.WorkspaceRoleId && r.WorkspaceId == request.WorkspaceId && r.Workspace.OrganizationId == _currentUser.OrganizationId,
            cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound($"Workspace role {request.WorkspaceRoleId} was not found in workspace {request.WorkspaceId}."));
        }

        // Users is ITenantScoped, so this is already implicitly filtered to the current
        // organization by the global query filter.
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure(Error.NotFound($"User {request.UserId} was not found."));
        }

        var alreadyAssigned = await _db.WorkspaceRoleAssignments.AnyAsync(
            a => a.WorkspaceId == request.WorkspaceId && a.UserId == request.UserId && a.WorkspaceRoleId == request.WorkspaceRoleId,
            cancellationToken);

        if (!alreadyAssigned)
        {
            _db.WorkspaceRoleAssignments.Add(new WorkspaceRoleAssignment
            {
                WorkspaceId = request.WorkspaceId,
                UserId = request.UserId,
                WorkspaceRoleId = request.WorkspaceRoleId,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
