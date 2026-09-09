using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.UpdateWorkspaceRole;

public class UpdateWorkspaceRoleCommandHandler : IRequestHandler<UpdateWorkspaceRoleCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public UpdateWorkspaceRoleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateWorkspaceRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _db.WorkspaceRoles
            .FirstOrDefaultAsync(r => r.Id == request.WorkspaceRoleId && r.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(Error.NotFound($"Workspace role {request.WorkspaceRoleId} was not found."));
        }

        var nameTaken = await _db.WorkspaceRoles.AnyAsync(
            r => r.WorkspaceId == role.WorkspaceId && r.Name == request.Name && r.Id != role.Id, cancellationToken);

        if (nameTaken)
        {
            return Result.Failure(Error.Conflict($"A role named \"{request.Name}\" already exists in this workspace."));
        }

        role.Name = request.Name;
        role.Permissions = string.Join(',', request.Permissions);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
