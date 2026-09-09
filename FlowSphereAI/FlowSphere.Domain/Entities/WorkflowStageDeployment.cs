using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>Which WorkflowVersion is currently active at a given sandbox stage for a
/// WorkflowDefinition. Independent stages can point at different versions simultaneously (unlike
/// WorkflowVersion.Status == Published, which is a single global pointer) - promoting a version to
/// a stage upserts the row for (WorkflowDefinitionId, Stage).</summary>
public class WorkflowStageDeployment : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    public EnvironmentStage Stage { get; set; }
    public int WorkflowVersionId { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowVersion WorkflowVersion { get; set; } = null!;
}
