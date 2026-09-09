using FlowSphere.Application.Common;
using FlowSphere.Domain.Enums;
using MediatR;

namespace FlowSphere.Application.Apps.Queries.GetDeploymentLog;

/// <summary>Stage is optional - when provided, only entries where this stage was either the
/// source or destination of the move are returned (matches the Deployments list page's own
/// "only show what's relevant to the currently selected stage" filtering, rather than dumping
/// every stage's history into one undifferentiated list).</summary>
public record GetDeploymentLogQuery(EnvironmentStage? Stage = null) : IRequest<Result<List<DeploymentLogEntryDto>>>;

public record DeploymentLogEntryDto(
    int Id,
    string AppName,
    string FromStage,
    string ToStage,
    string Action,
    string PerformedByUserName,
    DateTime CreatedDate);
