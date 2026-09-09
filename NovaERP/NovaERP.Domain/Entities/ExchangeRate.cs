namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped so each organization can customize its own conversion rates rather
/// than sharing a single global rate table.</summary>
public class ExchangeRate : BaseEntity
{
    public int      OrganizationId   { get; set; }
    public string   FromCurrencyCode { get; set; } = string.Empty;
    public string   ToCurrencyCode   { get; set; } = string.Empty;
    public decimal  Rate             { get; set; }
    public DateTime EffectiveDate    { get; set; }

    public Organization Organization { get; set; } = null!;
}
