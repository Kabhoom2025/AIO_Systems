namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped physical stock location tied to a Branch. StockMovement can
/// optionally reference one via WarehouseId, enabling multi-location stock tracking.</summary>
public class Warehouse : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int     BranchId       { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string  Code           { get; set; } = string.Empty;
    public string? ContactName    { get; set; }
    public string? Email          { get; set; }
    public string? Phone          { get; set; }
    public string? Address        { get; set; }
    public string? AddressLine2   { get; set; }
    public string? City           { get; set; }
    public string? State          { get; set; }
    public string? PostalCode     { get; set; }
    public string? Country        { get; set; }
    public string? TaxType        { get; set; }
    public string? TaxCountry     { get; set; }
    public string? TaxId          { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}
