namespace NovaERP.Domain.Entities;

/// <summary>One rate line within a <see cref="TaxCode"/> (e.g. "CGST" at 9%). A tax code with
/// a single component is a simple flat-rate tax; more than one models a compound tax.</summary>
public class TaxComponent : BaseEntity
{
    public int     TaxCodeId    { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public decimal RatePercent  { get; set; }
    public int     DisplayOrder { get; set; }

    public TaxCode TaxCode { get; set; } = null!;
}
