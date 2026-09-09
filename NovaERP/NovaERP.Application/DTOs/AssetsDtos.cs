namespace NovaERP.Application.DTOs;

public class AssetCategoryDto
{
    public int    Id       { get; set; }
    public string Name     { get; set; } = string.Empty;
    public string Code     { get; set; } = string.Empty;
    public bool   IsActive { get; set; }
}

public class CreateAssetCategoryDto
{
    public string Name     { get; set; } = string.Empty;
    public string Code     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as Product.Sku/Warehouse.Code.</summary>
public class UpdateAssetCategoryDto
{
    public string Name     { get; set; } = string.Empty;
    public bool   IsActive { get; set; } = true;
}

public class AssetDto
{
    public int     Id             { get; set; }
    public string  AssetCode      { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public int     CategoryId     { get; set; }
    public string  CategoryName   { get; set; } = string.Empty;
    public string? SerialNumber   { get; set; }

    public DateTime  PurchaseDate       { get; set; }
    public decimal?  PurchaseCost       { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public int?    AssignedToId   { get; set; }
    public string? AssignedToName { get; set; }
    public string  Status         { get; set; } = string.Empty;
}

public class CreateAssetDto
{
    public string  Name         { get; set; } = string.Empty;
    public int     CategoryId   { get; set; }
    public string? SerialNumber { get; set; }

    public DateTime  PurchaseDate       { get; set; } = DateTime.UtcNow.Date;
    public decimal?  PurchaseCost       { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
}

public class UpdateAssetDto
{
    public string  Name         { get; set; } = string.Empty;
    public int     CategoryId   { get; set; }
    public string? SerialNumber { get; set; }

    public DateTime  PurchaseDate       { get; set; }
    public decimal?  PurchaseCost       { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
}

public class AssignAssetDto
{
    public int EmployeeId { get; set; }
}
