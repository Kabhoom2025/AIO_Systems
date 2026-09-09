using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.WorkspaceRoles.Queries.GetWorkspaceRoles;

public class GetWorkspaceRolesQueryHandler : IRequestHandler<GetWorkspaceRolesQuery, Result<IReadOnlyList<WorkspaceRoleDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetWorkspaceRolesQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<WorkspaceRoleDto>>> Handle(GetWorkspaceRolesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _db.WorkspaceRoles
            .Include(r => r.Workspace)
            .Where(r => r.Workspace.OrganizationId == _currentUser.OrganizationId
                && (request.WorkspaceId == null || r.WorkspaceId == request.WorkspaceId))
            .OrderBy(r => r.Workspace.Name).ThenBy(r => r.Name)
            .Select(r => new { r.Id, r.WorkspaceId, WorkspaceName = r.Workspace.Name, r.Name, r.Permissions })
            .ToListAsync(cancellationToken);

        // PermissionList (string.Split) can't be translated to SQL, so it's mapped in memory
        // after materializing the raw Permissions column.
        var roles = rows
            .Select(r => new WorkspaceRoleDto(r.Id, r.WorkspaceId, r.WorkspaceName, r.Name,
                r.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()))
            .ToList();

        return Result<IReadOnlyList<WorkspaceRoleDto>>.Success(roles);
    }
}
