namespace NovaERP.Application.DTOs;

public class StoreDto
{
    public int     Id            { get; set; }
    public int     BranchId      { get; set; }
    public string  BranchName    { get; set; } = string.Empty;
    public int     WarehouseId   { get; set; }
    public string  WarehouseName { get; set; } = string.Empty;
    public string  Name          { get; set; } = string.Empty;
    public string  Code          { get; set; } = string.Empty;
    public string? Address       { get; set; }
    public bool    IsActive      { get; set; }
}

public class CreateStoreDto
{
    public int     BranchId    { get; set; }
    public int     WarehouseId { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  Code        { get; set; } = string.Empty;
    public string? Address     { get; set; }
    public bool    IsActive    { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as Warehouse.Code/AssetCategory.Code.</summary>
public class UpdateStoreDto
{
    public int     BranchId    { get; set; }
    public int     WarehouseId { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string? Address     { get; set; }
    public bool    IsActive    { get; set; } = true;
}
