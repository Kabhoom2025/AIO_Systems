using FlowSphere.Application.Common;
using FlowSphere.Application.Interfaces;
using FlowSphere.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Application.Workflows.Commands.PromoteWorkflowVersion;

/// <summary>Upserts which WorkflowVersion is active at a given sandbox stage for a
/// WorkflowDefinition - independent stages can point at different versions simultaneously,
/// unlike the single global VersionStatus.Published pointer WorkflowDefinition.Publish() sets.</summary>
public class PromoteWorkflowVersionCommandHandler : IRequestHandler<PromoteWorkflowVersionCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public PromoteWorkflowVersionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(PromoteWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        // WorkflowDefinitions/WorkflowVersions are both ITenantScoped, so these are already
        // implicitly filtered to the current organization by the global query filter.
        var versionExists = await _db.WorkflowVersions
            .AnyAsync(v => v.Id == request.VersionId && v.WorkflowDefinitionId == request.WorkflowDefinitionId, cancellationToken);

        if (!versionExists)
        {
            return Result.Failure(Error.NotFound($"Version {request.VersionId} of workflow {request.WorkflowDefinitionId} was not found."));
        }

        var deployment = await _db.WorkflowStageDeployments
            .FirstOrDefaultAsync(d => d.WorkflowDefinitionId == request.WorkflowDefinitionId && d.Stage == request.TargetStage, cancellationToken);

        if (deployment is null)
        {
            _db.WorkflowStageDeployments.Add(new WorkflowStageDeployment
            {
                WorkflowDefinitionId = request.WorkflowDefinitionId,
                Stage = request.TargetStage,
                WorkflowVersionId = request.VersionId,
            });
        }
        else
        {
            deployment.WorkflowVersionId = request.VersionId;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
