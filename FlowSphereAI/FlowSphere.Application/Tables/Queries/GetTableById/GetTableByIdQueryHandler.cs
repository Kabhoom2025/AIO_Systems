using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Tables.Queries.GetTableById;

public class GetTableByIdQueryHandler : IRequestHandler<GetTableByIdQuery, Result<TableDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetTableByIdQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<TableDetailDto>> Handle(GetTableByIdQuery request, CancellationToken cancellationToken)
    {
        var table = await _db.TableDefinitions
            .Where(t => t.Id == request.Id && t.Workspace.OrganizationId == _currentUser.OrganizationId)
            .Select(t => new TableDetailDto(t.Id, t.WorkspaceId, t.Name, t.Description, t.SchemaJson, t.IsPublished, t.PublishedAt, t.CreatedDate))
            .FirstOrDefaultAsync(cancellationToken);

        return table is null
            ? Result<TableDetailDto>.Failure(Error.NotFound($"Table {request.Id} was not found."))
            : Result<TableDetailDto>.Success(table);
    }
}
