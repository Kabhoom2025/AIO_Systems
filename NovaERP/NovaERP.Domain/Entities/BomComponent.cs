namespace NovaERP.Domain.Entities;

/// <summary>One component line within a BillOfMaterial (e.g. "4x Steel Sheet per Rack").
/// Quantity is per 1 unit of the finished good.</summary>
public class BomComponent : BaseEntity
{
    public int     BillOfMaterialId  { get; set; }
    public int     ComponentProductId { get; set; }
    public decimal Quantity          { get; set; }
    public int     DisplayOrder      { get; set; }

    public BillOfMaterial BillOfMaterial { get; set; } = null!;
    public Product ComponentProduct { get; set; } = null!;
}
