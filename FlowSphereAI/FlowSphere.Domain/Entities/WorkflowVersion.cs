using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

public class WorkflowVersion : BaseEntity
{
    public int WorkflowDefinitionId { get; set; }
    public int VersionNumber { get; set; }
    public VersionStatus Status { get; set; } = VersionStatus.Draft;

    /// <summary>Nodes + edges as authored on the React Flow canvas - source of truth for the
    /// execution engine in Phase 1 (WorkflowNode/WorkflowEdge rows, added in a later phase, would
    /// be a denormalized read model derived from this, not the other way around).</summary>
    public string GraphJson { get; set; } = "{\"nodes\":[],\"edges\":[]}";

    public DateTime? PublishedAt { get; set; }

    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    internal void MarkPublished()
    {
        Status = VersionStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    internal void Archive()
    {
        Status = VersionStatus.Archived;
    }
}
