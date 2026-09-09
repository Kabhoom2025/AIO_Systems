namespace NovaERP.Application.DTOs;

public class WarehouseDto
{
    public int     Id           { get; set; }
    public int     BranchId     { get; set; }
    public string  BranchName   { get; set; } = string.Empty;
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? ContactName  { get; set; }
    public string? Email        { get; set; }
    public string? Phone        { get; set; }
    public string? Address      { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? PostalCode   { get; set; }
    public string? Country      { get; set; }
    public string? TaxType      { get; set; }
    public string? TaxCountry   { get; set; }
    public string? TaxId        { get; set; }
    public bool    IsActive     { get; set; }
}

public class CreateWarehouseDto
{
    public int     BranchId     { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string  Code         { get; set; } = string.Empty;
    public string? ContactName  { get; set; }
    public string? Email        { get; set; }
    public string? Phone        { get; set; }
    public string? Address      { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? PostalCode   { get; set; }
    public string? Country      { get; set; }
    public string? TaxType      { get; set; }
    public string? TaxCountry   { get; set; }
    public string? TaxId        { get; set; }
    public bool    IsActive     { get; set; } = true;
}

/// <summary>Code is immutable after creation — same convention as Product.Sku.</summary>
public class UpdateWarehouseDto
{
    public int     BranchId     { get; set; }
    public string  Name         { get; set; } = string.Empty;
    public string? ContactName  { get; set; }
    public string? Email        { get; set; }
    public string? Phone        { get; set; }
    public string? Address      { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? PostalCode   { get; set; }
    public string? Country      { get; set; }
    public string? TaxType      { get; set; }
    public string? TaxCountry   { get; set; }
    public string? TaxId        { get; set; }
    public bool    IsActive     { get; set; } = true;
}

public class StockTransferDto
{
    public int      Id                { get; set; }
    public int      ProductId         { get; set; }
    public string   ProductName       { get; set; } = string.Empty;
    public string   ProductSku        { get; set; } = string.Empty;
    public int      FromWarehouseId   { get; set; }
    public string   FromWarehouseName { get; set; } = string.Empty;
    public int      ToWarehouseId     { get; set; }
    public string   ToWarehouseName   { get; set; } = string.Empty;
    public decimal  Quantity          { get; set; }
    public DateTime TransferDate      { get; set; }
    public string?  Notes             { get; set; }
}

public class CreateStockTransferDto
{
    public int      ProductId       { get; set; }
    public int      FromWarehouseId { get; set; }
    public int      ToWarehouseId   { get; set; }
    public decimal  Quantity        { get; set; }
    public string?  Notes           { get; set; }
}
