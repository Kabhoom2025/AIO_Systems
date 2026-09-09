namespace NovaERP.Domain.Entities;

/// <summary>System-wide reference data (not org-scoped) — the supported-languages list.
/// Translation-string storage is a later-phase i18n content-management concern; this phase
/// only tracks which languages exist and which ones an org supports (OrganizationLanguage).</summary>
public class Language : BaseEntity
{
    public string Code       { get; set; } = string.Empty;
    public string Name       { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public bool   IsRtl      { get; set; }
    public bool   IsActive   { get; set; } = true;
}
