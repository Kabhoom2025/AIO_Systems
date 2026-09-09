using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppTriggers;

public class GetAppTriggersQueryHandler : IRequestHandler<GetAppTriggersQuery, Result<List<AppTriggerDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetAppTriggersQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AppTriggerDto>>> Handle(GetAppTriggersQuery request, CancellationToken cancellationToken)
    {
        var triggers = await (
            from t in _db.AppTriggers
            join d in _db.AppDefinitions on t.DestinationAppId equals d.Id
            where t.SourceAppId == request.SourceAppId && t.OrganizationId == _currentUser.OrganizationId
            orderby t.Name
            select new AppTriggerDto(t.Id, t.Name, t.DestinationAppId, d.Name, t.ActionType, t.FieldMappingJson, t.IsEnabled)
        ).ToListAsync(cancellationToken);

        return Result<List<AppTriggerDto>>.Success(triggers);
    }
}
