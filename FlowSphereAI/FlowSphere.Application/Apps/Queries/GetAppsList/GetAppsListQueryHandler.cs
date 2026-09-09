using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppsList;

public class GetAppsListQueryHandler : IRequestHandler<GetAppsListQuery, Result<List<AppSummaryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public GetAppsListQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<List<AppSummaryDto>>> Handle(GetAppsListQuery request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<List<AppSummaryDto>>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var items = await _db.AppDefinitions
            .Where(a => a.WorkspaceId == request.WorkspaceId && a.Stage == _currentEnvironment.Stage)
            .OrderByDescending(a => a.CreatedDate)
            .Select(a => new AppSummaryDto(a.Id, a.Name, a.Description, a.Icon, a.IsPublished, a.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<List<AppSummaryDto>>.Success(items);
    }
}
