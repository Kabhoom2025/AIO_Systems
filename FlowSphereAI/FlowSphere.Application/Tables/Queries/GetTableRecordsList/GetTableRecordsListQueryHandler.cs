using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Queries.GetTableRecordsList;

public class GetTableRecordsListQueryHandler : IRequestHandler<GetTableRecordsListQuery, Result<List<TableRecordDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetTableRecordsListQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<TableRecordDto>>> Handle(GetTableRecordsListQuery request, CancellationToken cancellationToken)
    {
        var tableExists = await _db.TableDefinitions
            .AnyAsync(t => t.Id == request.TableId && t.Workspace.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (!tableExists)
        {
            return Result<List<TableRecordDto>>.Failure(Error.NotFound($"Table {request.TableId} was not found."));
        }

        var items = await _db.TableRecords
            .Where(r => r.TableDefinitionId == request.TableId)
            .OrderByDescending(r => r.CreatedDate)
            .Select(r => new TableRecordDto(r.Id, r.DataJson, r.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<List<TableRecordDto>>.Success(items);
    }
}
