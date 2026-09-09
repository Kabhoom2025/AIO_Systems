using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Queries.GetWorkspacesList;

public class GetWorkspacesListQueryHandler : IRequestHandler<GetWorkspacesListQuery, Result<List<WorkspaceSummaryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetWorkspacesListQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<WorkspaceSummaryDto>>> Handle(GetWorkspacesListQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await _db.Workspaces
            .Where(w => w.OrganizationId == _currentUser.OrganizationId)
            .OrderByDescending(w => w.CreatedDate)
            .Select(w => new { w.Id, w.Name, w.Description, AppCount = w.Apps.Count, w.CreatedDate })
            .ToListAsync(cancellationToken);

        var memberships = await _db.WorkspaceMemberships
            .Where(m => workspaces.Select(w => w.Id).Contains(m.WorkspaceId))
            .Select(m => new { m.WorkspaceId, m.UserId, m.Role, UserName = m.User.Name })
            .ToListAsync(cancellationToken);

        var items = workspaces
            .Select(w => new WorkspaceSummaryDto(
                w.Id, w.Name, w.Description, w.AppCount, w.CreatedDate,
                memberships.Where(m => m.WorkspaceId == w.Id && m.Role == WorkspaceAdminRole.WorkspaceAdmin)
                    .Select(m => new AdminUserDto(m.UserId, m.UserName)).ToList(),
                memberships.Where(m => m.WorkspaceId == w.Id && m.Role == WorkspaceAdminRole.DataAdmin)
                    .Select(m => new AdminUserDto(m.UserId, m.UserName)).ToList()))
            .ToList();

        return Result<List<WorkspaceSummaryDto>>.Success(items);
    }
}
