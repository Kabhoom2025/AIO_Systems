using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>A single end-user comment on a submitted AppRecord, only surfaced when the app's
/// SettingsJson.enableComments flag is on. CreatedDate (BaseEntity) is the comment timestamp.</summary>
public class AppRecordComment : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public int AppRecordId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
