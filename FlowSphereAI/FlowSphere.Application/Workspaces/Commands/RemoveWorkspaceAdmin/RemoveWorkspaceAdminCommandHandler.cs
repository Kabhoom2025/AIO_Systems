using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Commands.RemoveWorkspaceAdmin;

public class RemoveWorkspaceAdminCommandHandler : IRequestHandler<RemoveWorkspaceAdminCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public RemoveWorkspaceAdminCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RemoveWorkspaceAdminCommand request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var membership = await _db.WorkspaceMemberships.FirstOrDefaultAsync(
            m => m.WorkspaceId == request.WorkspaceId && m.UserId == request.UserId && m.Role == request.Role,
            cancellationToken);

        if (membership is not null)
        {
            _db.WorkspaceMemberships.Remove(membership);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
