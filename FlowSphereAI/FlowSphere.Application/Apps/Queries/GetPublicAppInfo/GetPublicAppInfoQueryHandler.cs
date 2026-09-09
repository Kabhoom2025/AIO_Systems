using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetPublicAppInfo;

public class GetPublicAppInfoQueryHandler : IRequestHandler<GetPublicAppInfoQuery, Result<PublicAppInfoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPublicAppInfoQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<PublicAppInfoDto>> Handle(GetPublicAppInfoQuery request, CancellationToken cancellationToken)
    {
        // No organization scoping here - there is no authenticated caller to scope by, and
        // AppDefinition isn't itself tenant-scoped (it's reached transitively through Workspace
        // everywhere else). IsPublished is the only gate: an unpublished app's name isn't exposed.
        var app = await _db.AppDefinitions
            .Where(a => a.Id == request.Id && a.IsPublished)
            .Select(a => new PublicAppInfoDto(a.Name, a.Description))
            .FirstOrDefaultAsync(cancellationToken);

        return app is null
            ? Result<PublicAppInfoDto>.Failure(Error.NotFound($"App {request.Id} was not found."))
            : Result<PublicAppInfoDto>.Success(app);
    }
}
