namespace NovaERP.Domain.Entities;

/// <summary>Join between Organization and Language — which languages an org supports and
/// which one is the default (OrganizationSettings.DefaultLanguageCode should match it).</summary>
public class OrganizationLanguage : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string LanguageCode   { get; set; } = string.Empty;
    public bool   IsDefault      { get; set; }

    public Organization Organization { get; set; } = null!;
}
