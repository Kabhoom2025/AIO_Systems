using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>Audit trail row for one Publish action, written alongside AppDefinition.Publish() in
/// PublishAppCommandHandler. AppId is the Dev-stage AppDefinition.Id that was actually published
/// (Publish is Dev-only, see PublishAppCommandHandler's stage guard) - CreatedDate (from BaseEntity)
/// is the publish timestamp, no separate field needed, same convention as DeploymentLogEntry.</summary>
public class AppPublishHistoryEntry : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public int AppId { get; set; }
    public string? Comment { get; set; }
    public int PublishedByUserId { get; set; }
    public string PublishedByUserName { get; set; } = string.Empty;
}
