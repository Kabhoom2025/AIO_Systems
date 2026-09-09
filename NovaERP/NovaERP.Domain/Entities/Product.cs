namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped catalog item. Shared reference data (like TaxCode/Currency), not
/// owned by a rep — Sales/Purchase order lines can optionally reference one instead of (or
/// alongside) their free-text ItemName.</summary>
public class Product : BaseEntity
{
    public int     OrganizationId { get; set; }
    public string  Sku            { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public string? Description    { get; set; }
    public string  UnitOfMeasure  { get; set; } = "EA";
    public decimal UnitCost       { get; set; }
    public decimal? WeightKg      { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
