using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Queries.GetUserRoleAssignments;

public class GetUserRoleAssignmentsQueryHandler : IRequestHandler<GetUserRoleAssignmentsQuery, Result<IReadOnlyList<UserRoleAssignmentDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetUserRoleAssignmentsQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<UserRoleAssignmentDto>>> Handle(GetUserRoleAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<IReadOnlyList<UserRoleAssignmentDto>>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var matchingUserIds = request.WorkspaceRoleId is null
            ? null
            : await _db.WorkspaceRoleAssignments
                .Where(a => a.WorkspaceId == request.WorkspaceId && a.WorkspaceRoleId == request.WorkspaceRoleId)
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);

        var rows = await _db.WorkspaceRoleAssignments
            .Include(a => a.User)
            .Include(a => a.WorkspaceRole)
            .Where(a => a.WorkspaceId == request.WorkspaceId && (matchingUserIds == null || matchingUserIds.Contains(a.UserId)))
            .Select(a => new { a.UserId, a.User.Name, a.User.Email, a.WorkspaceRoleId, RoleName = a.WorkspaceRole.Name })
            .ToListAsync(cancellationToken);

        var result = rows
            .GroupBy(r => new { r.UserId, r.Name, r.Email })
            .Select(g => new UserRoleAssignmentDto(
                g.Key.UserId,
                g.Key.Name,
                g.Key.Email,
                g.Select(r => new AssignedRoleDto(r.WorkspaceRoleId, r.RoleName)).ToList()))
            .OrderBy(u => u.UserName)
            .ToList();

        return Result<IReadOnlyList<UserRoleAssignmentDto>>.Success(result);
    }
}
