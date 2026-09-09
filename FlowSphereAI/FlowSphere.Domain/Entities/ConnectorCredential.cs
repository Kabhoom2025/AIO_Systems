using FlowSphere.Domain.Common;

namespace FlowSphere.Domain.Entities;

/// <summary>EncryptedValue is DPAPI/IDataProtector-encrypted at rest in Phase 1 (vault
/// integration deferred). Never returned in DTOs after write - only KeyName/metadata are.</summary>
public class ConnectorCredential : BaseEntity, ITenantScoped
{
    public int OrganizationId { get; set; }
    public int ConnectorId { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public string EncryptedValue { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }

    public Connector Connector { get; set; } = null!;
}
