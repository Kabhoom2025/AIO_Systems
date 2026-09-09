namespace NovaERP.Domain.Entities;

/// <summary>Org-scoped physical retail location, tied to a Branch (same shape as Warehouse)
/// plus a link to the Warehouse that backs its stock. Plain CRUD, no lifecycle — reference
/// data like Warehouse/AssetCategory/TicketCategory.</summary>
public class Store : BaseEntity
{
    public int     OrganizationId { get; set; }
    public int     BranchId       { get; set; }
    public int     WarehouseId    { get; set; }
    public string  Name           { get; set; } = string.Empty;
    public string  Code           { get; set; } = string.Empty;
    public string? Address        { get; set; }
    public bool    IsActive       { get; set; } = true;

    public Organization Organization { get; set; } = null!;
    public Branch    Branch    { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
