namespace HRMS.Domain.Entities;

public class Asset : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int?     BranchId       { get; set; }
    public string   AssetTag       { get; set; } = string.Empty; // e.g. AST-0001
    public string   Name           { get; set; } = string.Empty;
    public string   Category       { get; set; } = "Laptop"; // Laptop | Desktop | Mobile | SIM | Vehicle | IDCard | Furniture | Other
    public string?  SerialNumber   { get; set; }
    public DateOnly? PurchaseDate  { get; set; }
    public decimal? PurchaseCost   { get; set; }
    public DateOnly? WarrantyUntil { get; set; }
    public string   Condition      { get; set; } = "Good"; // New | Good | Fair | Damaged
    public string   Status         { get; set; } = "Available"; // Available | Allocated | InRepair | Retired
    public string?  Notes          { get; set; }

    public Organization Organization { get; set; } = null!;
    public Branch?      Branch       { get; set; }
    public ICollection<AssetAllocation> Allocations { get; set; } = new List<AssetAllocation>();
}

public class AssetAllocation : BaseEntity
{
    public int      AssetId           { get; set; }
    public int      EmployeeId        { get; set; }
    public DateOnly AllocatedDate     { get; set; }
    public DateOnly? ReturnedDate     { get; set; }
    public int?     AllocatedByUserId { get; set; }
    public string?  ReturnCondition   { get; set; }
    public string?  Notes             { get; set; }

    public Asset    Asset             { get; set; } = null!;
    public Employee Employee          { get; set; } = null!;
    public User?    AllocatedByUser   { get; set; }
}
