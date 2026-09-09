using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetAppById;

public class GetAppByIdQueryHandler : IRequestHandler<GetAppByIdQuery, Result<AppDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly ICurrentEnvironmentContext _currentEnvironment;

    public GetAppByIdQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser, ICurrentEnvironmentContext currentEnvironment)
    {
        _db = db;
        _currentUser = currentUser;
        _currentEnvironment = currentEnvironment;
    }

    public async Task<Result<AppDetailDto>> Handle(GetAppByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await _db.AppDefinitions
            .Where(a => a.Id == request.Id && a.Workspace.OrganizationId == _currentUser.OrganizationId && a.Stage == _currentEnvironment.Stage)
            .Select(a => new AppDetailDto(
                a.Id, a.WorkspaceId, a.Name, a.Description, a.Icon, a.FormSchemaJson,
                a.LinkedWorkflowDefinitionId, a.LinkedTableId, a.FieldMappingJson,
                a.BusinessRulesJson, a.AccessPermissionsJson,
                a.IsPublished, a.PublishedAt, a.CreatedDate,
                a.SettingsJson, a.UserManualMarkdown, a.PublishedSnapshotJson))
            .FirstOrDefaultAsync(cancellationToken);

        return app is null
            ? Result<AppDetailDto>.Failure(Error.NotFound($"App {request.Id} was not found."))
            : Result<AppDetailDto>.Success(app);
    }
}
