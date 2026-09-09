using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Commands.CreateWorkspaceRole;

public class CreateWorkspaceRoleCommandHandler : IRequestHandler<CreateWorkspaceRoleCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public CreateWorkspaceRoleCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateWorkspaceRoleCommand request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<int>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var nameTaken = await _db.WorkspaceRoles
            .AnyAsync(r => r.WorkspaceId == request.WorkspaceId && r.Name == request.Name, cancellationToken);

        if (nameTaken)
        {
            return Result<int>.Failure(Error.Conflict($"A role named \"{request.Name}\" already exists in this workspace."));
        }

        var role = new WorkspaceRole
        {
            WorkspaceId = request.WorkspaceId,
            Name = request.Name,
            Permissions = string.Join(',', request.Permissions),
        };
        _db.WorkspaceRoles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(role.Id);
    }
}
