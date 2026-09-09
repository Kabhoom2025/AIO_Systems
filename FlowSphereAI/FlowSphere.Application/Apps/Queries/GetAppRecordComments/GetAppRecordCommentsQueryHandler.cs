using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppRecordComments;

public class GetAppRecordCommentsQueryHandler : IRequestHandler<GetAppRecordCommentsQuery, Result<List<AppRecordCommentDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetAppRecordCommentsQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AppRecordCommentDto>>> Handle(GetAppRecordCommentsQuery request, CancellationToken cancellationToken)
    {
        var recordExists = await _db.AppRecords
            .AnyAsync(r => r.Id == request.AppRecordId && r.AppDefinitionId == request.AppId, cancellationToken);
        if (!recordExists)
        {
            return Result<List<AppRecordCommentDto>>.Failure(Error.NotFound($"Record {request.AppRecordId} was not found."));
        }

        var items = await _db.AppRecordComments
            .Where(c => c.AppRecordId == request.AppRecordId && c.OrganizationId == _currentUser.OrganizationId)
            .OrderBy(c => c.CreatedDate)
            .Select(c => new AppRecordCommentDto(c.Id, c.UserName, c.Text, c.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<List<AppRecordCommentDto>>.Success(items);
    }
}
