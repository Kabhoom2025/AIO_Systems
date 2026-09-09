using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Commands.DeleteWorkspace;

/// <summary>Cascade-deletes every app/table (at every sandbox stage) and their records via the
/// existing FK configurations on AppDefinition/TableDefinition's Workspace relationship - a plain
/// remove-and-save, no manual cascade logic needed here.</summary>
public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DeleteWorkspaceCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _db.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (workspace is null)
        {
            return Result.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        _db.Workspaces.Remove(workspace);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
