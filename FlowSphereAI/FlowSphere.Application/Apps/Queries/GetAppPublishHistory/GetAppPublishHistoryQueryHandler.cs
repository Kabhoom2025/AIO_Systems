using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppPublishHistory;

public class GetAppPublishHistoryQueryHandler : IRequestHandler<GetAppPublishHistoryQuery, Result<List<AppPublishHistoryEntryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAppPublishHistoryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<AppPublishHistoryEntryDto>>> Handle(GetAppPublishHistoryQuery request, CancellationToken cancellationToken)
    {
        // AppPublishHistoryEntries is ITenantScoped, so this is already implicitly filtered to the
        // current organization by the global query filter - the AppId filter alone is safe.
        var dtos = await _db.AppPublishHistoryEntries
            .Where(p => p.AppId == request.AppId)
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new AppPublishHistoryEntryDto(p.Id, p.Comment, p.PublishedByUserName, p.CreatedDate))
            .ToListAsync(cancellationToken);

        return Result<List<AppPublishHistoryEntryDto>>.Success(dtos);
    }
}
