using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Queries.GetTablesList;

public class GetTablesListQueryHandler : IRequestHandler<GetTablesListQuery, Result<List<TableSummaryDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetTablesListQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<TableSummaryDto>>> Handle(GetTablesListQuery request, CancellationToken cancellationToken)
    {
        var workspaceExists = await _db.Workspaces
            .AnyAsync(w => w.Id == request.WorkspaceId && w.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!workspaceExists)
        {
            return Result<List<TableSummaryDto>>.Failure(Error.NotFound($"Workspace {request.WorkspaceId} was not found."));
        }

        var items = await _db.TableDefinitions
            .Where(t => t.WorkspaceId == request.WorkspaceId)
            .OrderByDescending(t => t.CreatedDate)
            .Select(t => new TableSummaryDto(t.Id, t.Name, t.Description, t.IsPublished, t.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<List<TableSummaryDto>>.Success(items);
    }
}
