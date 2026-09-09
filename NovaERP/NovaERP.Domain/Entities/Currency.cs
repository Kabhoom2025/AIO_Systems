namespace NovaERP.Domain.Entities;

/// <summary>System-wide reference data (not org-scoped) — the list of currencies an org can
/// pick from via OrganizationSettings.DefaultCurrencyCode or use in ExchangeRate.</summary>
public class Currency : BaseEntity
{
    public string Code          { get; set; } = string.Empty;
    public string Name          { get; set; } = string.Empty;
    public string Symbol        { get; set; } = string.Empty;
    public int    DecimalPlaces { get; set; } = 2;
    public bool   IsActive      { get; set; } = true;
}
