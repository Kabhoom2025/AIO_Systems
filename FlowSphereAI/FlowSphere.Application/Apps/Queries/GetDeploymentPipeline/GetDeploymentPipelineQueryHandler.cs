using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Apps.Queries.GetDeploymentPipeline;

public class GetDeploymentPipelineQueryHandler : IRequestHandler<GetDeploymentPipelineQuery, Result<List<DeploymentAppDto>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetDeploymentPipelineQueryHandler(IApplicationDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<List<DeploymentAppDto>>> Handle(GetDeploymentPipelineQuery request, CancellationToken cancellationToken)
    {
        var items = await _db.AppDefinitions
            .Where(a => a.Workspace.OrganizationId == _currentUser.OrganizationId)
            .OrderBy(a => a.SourceGroupId)
            .ThenBy(a => a.Stage)
            .Select(a => new
            {
                a.Id, a.SourceGroupId, a.Name, a.WorkspaceId, WorkspaceName = a.Workspace.Name, a.Stage, a.IsPublished, a.UpdatedDate,
            })
            .ToListAsync(cancellationToken);

        var dtos = items
            .Select(a => new DeploymentAppDto(
                a.Id, a.SourceGroupId, a.Name, a.WorkspaceId, a.WorkspaceName, a.Stage.ToString(), a.IsPublished,
                // UpdatedDate, not CreatedDate - a re-promoted row (ApplyPromotedConfig updating
                // an existing target-stage row in place) keeps its original CreatedDate but bumps
                // UpdatedDate, so this is the only field that reflects the latest move accurately.
                a.UpdatedDate))
            .ToList();

        return Result<List<DeploymentAppDto>>.Success(dtos);
    }
}
