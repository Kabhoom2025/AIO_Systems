namespace NovaERP.Domain.Entities;

/// <summary>One product line within a PosSale. LineTotal (Quantity * UnitPrice) is computed on
/// read, never stored — same precedent as ProductDto.OnHandQuantity/LedgerAccountDto.Balance.</summary>
public class PosSaleLine : BaseEntity
{
    public int     PosSaleId { get; set; }
    public int     ProductId { get; set; }
    public decimal Quantity  { get; set; }
    public decimal UnitPrice { get; set; }

    public PosSale PosSale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
