using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.DeleteWorkspaceRole;

/// <summary>Cascade-deletes any WorkspaceRoleAssignment rows granting this role via the existing
/// FK configuration - a plain remove-and-save.</summary>
public class DeleteWorkspaceRoleCommandHandler : IRequestHandler<DeleteWorkspaceRoleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DeleteWorkspaceRoleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteWorkspaceRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _db.WorkspaceRoles
            .FirstOrDefaultAsync(r => r.Id == request.WorkspaceRoleId && r.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound($"Workspace role {request.WorkspaceRoleId} was not found."));
        }

        _db.WorkspaceRoles.Remove(role);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
