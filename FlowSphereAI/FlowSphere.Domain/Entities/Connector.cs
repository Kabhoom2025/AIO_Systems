using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>OrganizationId is nullable: null = platform-provided built-in connector (Http, Smtp,
/// OpenAi), visible to every tenant; non-null = an org-specific instance (not used in Phase 1,
/// reserved for a later "bring your own connector" phase).</summary>
public class Connector : BaseEntity
{
    public int? OrganizationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;

    public ICollection<ConnectorCredential> Credentials { get; set; } = new List<ConnectorCredential>();
}
