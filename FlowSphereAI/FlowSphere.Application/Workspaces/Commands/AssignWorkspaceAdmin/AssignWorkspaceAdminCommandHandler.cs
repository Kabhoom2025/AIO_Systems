using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Commands.AssignWorkspaceAdmin;

public class AssignWorkspaceAdminCommandHandler : IRequestHandler<AssignWorkspaceAdminCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public AssignWorkspaceAdminCommandHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AssignWorkspaceAdminCommand request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        // Users is ITenantScoped, so this is already implicitly filtered to the current
        // organization by the global query filter.
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure(Error.NotFound($"User {request.UserId} was not found."));
        }

        var alreadyAssigned = await _db.WorkspaceMemberships.AnyAsync(
            m => m.WorkspaceId == request.WorkspaceId && m.UserId == request.UserId && m.Role == request.Role,
            cancellationToken);

        if (!alreadyAssigned)
        {
            _db.WorkspaceMemberships.Add(new WorkspaceMembership
            {
                WorkspaceId = request.WorkspaceId,
                UserId = request.UserId,
                Role = request.Role,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
