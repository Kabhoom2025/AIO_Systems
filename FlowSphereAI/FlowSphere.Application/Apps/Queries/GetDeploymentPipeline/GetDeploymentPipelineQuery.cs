using FlowSphere.Application.Common;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetDeploymentPipeline;

/// <summary>Every app across every workspace in the org, one row per stage it currently exists
/// at - powers the Deployments pipeline page's Dev/QA/UAT/Live columns. Unlike GetAppsListQuery
/// (one workspace, current stage only), this is org-wide and spans all four stages at once so
/// the page can show the whole pipeline in one view.</summary>
public record GetDeploymentPipelineQuery : IRequest<Result<List<DeploymentAppDto>>>;

public record DeploymentAppDto(
    int Id,
    Guid SourceGroupId,
    string Name,
    int WorkspaceId,
    string WorkspaceName,
    string Stage,
    bool IsPublished,
    DateTime MovedDate);
