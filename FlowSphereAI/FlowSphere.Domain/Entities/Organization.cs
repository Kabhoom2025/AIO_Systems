using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string PlanTier { get; set; } = "Free";
    public int MonthlyExecutionQuota { get; set; } = 100;

    /// <summary>Platform-wide preferences (today: { "backgroundImageDataUri": "data:image/..." }
    /// applied behind every page) - same shallow-JSON-blob convention used for app SettingsJson.
    /// No file-storage service exists in this system, so the image is embedded directly as a data
    /// URI rather than uploaded to a CDN/blob store - fine for a modest background image, not
    /// meant for large files (see SaveOrganizationSettingsCommandHandler's size cap).</summary>
    public string SettingsJson { get; set; } = "{}";

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<WorkflowDefinition> Workflows { get; set; } = new List<WorkflowDefinition>();
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
