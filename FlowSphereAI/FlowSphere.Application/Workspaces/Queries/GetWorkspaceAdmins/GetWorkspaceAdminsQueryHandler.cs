using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workspaces.Queries.GetWorkspaceAdmins;

public class GetWorkspaceAdminsQueryHandler : IRequestHandler<GetWorkspaceAdminsQuery, Result<WorkspaceAdminsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetWorkspaceAdminsQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkspaceAdminsDto>> Handle(GetWorkspaceAdminsQuery request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<WorkspaceAdminsDto>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var memberships = await _db.WorkspaceMemberships
            .Where(m => m.WorkspaceId == request.WorkspaceId)
            .Select(m => new { m.UserId, m.Role, UserName = m.User.Name })
            .ToListAsync(cancellationToken);

        var dto = new WorkspaceAdminsDto(
            memberships.Where(m => m.Role == WorkspaceAdminRole.WorkspaceAdmin)
                .Select(m => new AdminUserDto(m.UserId, m.UserName)).ToList(),
            memberships.Where(m => m.Role == WorkspaceAdminRole.DataAdmin)
                .Select(m => new AdminUserDto(m.UserId, m.UserName)).ToList());

        return Result<WorkspaceAdminsDto>.Success(dto);
    }
}
