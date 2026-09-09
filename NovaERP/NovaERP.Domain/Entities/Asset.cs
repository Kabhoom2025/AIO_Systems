namespace NovaERP.Domain.Entities;

/// <summary>An org-scoped piece of tracked equipment. Assign/Unassign are explicit actions,
/// not a free-editable Status field like Project.Status — assigning inherently requires
/// specifying which Employee is receiving it, a real parameter, not just a status flip.
/// Retire is terminal; Update/Delete are locked once Retired.</summary>
public class Asset : BaseEntity
{
    public int    OrganizationId { get; set; }
    public string AssetCode      { get; set; } = string.Empty;
    public string Name           { get; set; } = string.Empty;
    public int    CategoryId     { get; set; }
    public string? SerialNumber  { get; set; }

    public DateTime  PurchaseDate       { get; set; }
    public decimal?  PurchaseCost       { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public int?   AssignedToId { get; set; }
    public string Status       { get; set; } = "Available"; // Available | Assigned | UnderMaintenance | Retired

    public Organization   Organization { get; set; } = null!;
    public AssetCategory  Category     { get; set; } = null!;
    public Employee?      AssignedTo   { get; set; }
}
