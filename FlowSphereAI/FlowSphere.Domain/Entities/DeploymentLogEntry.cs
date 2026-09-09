using FlowSphere.Domain.Common;
using FlowSphere.Domain.Enums;

namespace FlowSphere.Domain.Entities;

/// <summary>Audit trail row for one Promote or Rollback action - written alongside the actual
/// mutation in PromoteAppCommandHandler/RollbackAppCommandHandler. Snapshots AppName rather than
/// storing a hard FK to the row that changed, since Promote creates a new row and Rollback
/// deletes one - the log must survive both.</summary>
public class DeploymentLogEntry : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public Guid SourceGroupId { get; set; }
    public string AppName { get; set; } = string.Empty;
    public EnvironmentStage FromStage { get; set; }
    public EnvironmentStage ToStage { get; set; }
    public DeploymentAction Action { get; set; }
    public int PerformedByUserId { get; set; }

    public User PerformedByUser { get; set; } = null!;
}
