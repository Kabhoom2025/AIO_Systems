using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.RemoveWorkspaceRoleAssignment;

public class RemoveWorkspaceRoleAssignmentCommandHandler : IRequestHandler<RemoveWorkspaceRoleAssignmentCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public RemoveWorkspaceRoleAssignmentCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RemoveWorkspaceRoleAssignmentCommand request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var assignment = await _db.WorkspaceRoleAssignments.FirstOrDefaultAsync(
            a => a.WorkspaceId == request.WorkspaceId && a.UserId == request.UserId && a.WorkspaceRoleId == request.WorkspaceRoleId,
            cancellationToken);

        if (assignment is not null)
        {
            _db.WorkspaceRoleAssignments.Remove(assignment);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
