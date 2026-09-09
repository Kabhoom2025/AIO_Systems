namespace NovaERP.Application.DTOs;

public class ShipmentLineDto
{
    public int      Id               { get; set; }
    public int      ProductId        { get; set; }
    public string   ProductName      { get; set; } = string.Empty;
    public string   ProductSku       { get; set; } = string.Empty;
    public decimal  Quantity         { get; set; }
    public int?     SalesOrderLineId { get; set; }
    public int      DisplayOrder     { get; set; }
}

public class CreateShipmentLineDto
{
    public int     ProductId        { get; set; }
    public decimal Quantity         { get; set; }
    public int?    SalesOrderLineId { get; set; }
    public int     DisplayOrder     { get; set; }
}

public class ShipmentPackageItemDto
{
    public int     ProductId   { get; set; }
    public string  ProductName { get; set; } = string.Empty;
    public string  ProductSku  { get; set; } = string.Empty;
    public decimal Quantity    { get; set; }
}

public class CreateShipmentPackageItemDto
{
    public int     ProductId { get; set; }
    public decimal Quantity  { get; set; }
}

public class ShipmentPackageDto
{
    public int      Id             { get; set; }
    public int      PackageNumber  { get; set; }
    public decimal? WeightKg       { get; set; }
    public decimal? LengthCm       { get; set; }
    public decimal? WidthCm        { get; set; }
    public decimal? HeightCm       { get; set; }
    public string?  TrackingNumber { get; set; }
    public List<ShipmentPackageItemDto> Items { get; set; } = new();
}

public class CreateShipmentPackageDto
{
    public int      PackageNumber  { get; set; }
    public decimal? WeightKg       { get; set; }
    public decimal? LengthCm       { get; set; }
    public decimal? WidthCm        { get; set; }
    public decimal? HeightCm       { get; set; }
    public string?  TrackingNumber { get; set; }
    public List<CreateShipmentPackageItemDto> Items { get; set; } = new();
}

public class ShipmentDto
{
    public int      Id                     { get; set; }
    public string   ShipmentNumber         { get; set; } = string.Empty;
    public int      WarehouseId            { get; set; }
    public string   WarehouseName          { get; set; } = string.Empty;
    public string   SourceType             { get; set; } = string.Empty;
    public int?     SalesOrderId           { get; set; }
    public string?  SalesOrderNumber       { get; set; }
    public int?     DestinationWarehouseId { get; set; }
    public string?  DestinationWarehouseName { get; set; }

    public string? ShipToName         { get; set; }
    public string? ShipToContactName  { get; set; }
    public string? ShipToEmail        { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity         { get; set; }
    public string? ShipToState        { get; set; }
    public string? ShipToPostalCode   { get; set; }
    public string? ShipToCountry      { get; set; }
    public string? ShipToPhone        { get; set; }
    public string? ShipToTaxType      { get; set; }
    public string? ShipToTaxCountry   { get; set; }
    public string? ShipToTaxId        { get; set; }

    public string? ShipFromName         { get; set; }
    public string? ShipFromContactName  { get; set; }
    public string? ShipFromEmail        { get; set; }
    public string? ShipFromAddressLine1 { get; set; }
    public string? ShipFromAddressLine2 { get; set; }
    public string? ShipFromCity         { get; set; }
    public string? ShipFromState        { get; set; }
    public string? ShipFromPostalCode   { get; set; }
    public string? ShipFromCountry      { get; set; }
    public string? ShipFromPhone        { get; set; }

    public DateTime ShipDate        { get; set; }
    public string?  Carrier         { get; set; }
    public string?  TrackingNumber  { get; set; }
    public bool     IsBlindShipment { get; set; }
    public string   Status          { get; set; } = string.Empty;
    public int      OwnerId         { get; set; }
    public string   OwnerName       { get; set; } = string.Empty;

    public List<ShipmentLineDto> Lines       { get; set; } = new();
    public List<ShipmentPackageDto> Packages { get; set; } = new();
}

public class CreateShipmentDto
{
    public int      WarehouseId            { get; set; }
    public string   SourceType             { get; set; } = string.Empty;
    public int?     SalesOrderId           { get; set; }
    public int?     DestinationWarehouseId { get; set; }

    public string? ShipToName         { get; set; }
    public string? ShipToContactName  { get; set; }
    public string? ShipToEmail        { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity         { get; set; }
    public string? ShipToState        { get; set; }
    public string? ShipToPostalCode   { get; set; }
    public string? ShipToCountry      { get; set; }
    public string? ShipToPhone        { get; set; }
    public string? ShipToTaxType      { get; set; }
    public string? ShipToTaxCountry   { get; set; }
    public string? ShipToTaxId        { get; set; }

    public string? ShipFromName         { get; set; }
    public string? ShipFromContactName  { get; set; }
    public string? ShipFromEmail        { get; set; }
    public string? ShipFromAddressLine1 { get; set; }
    public string? ShipFromAddressLine2 { get; set; }
    public string? ShipFromCity         { get; set; }
    public string? ShipFromState        { get; set; }
    public string? ShipFromPostalCode   { get; set; }
    public string? ShipFromCountry      { get; set; }
    public string? ShipFromPhone        { get; set; }

    public DateTime ShipDate        { get; set; } = DateTime.UtcNow.Date;
    public string?  Carrier         { get; set; }
    public string?  TrackingNumber  { get; set; }
    public bool     IsBlindShipment { get; set; }
    public int      OwnerId         { get; set; }

    public List<CreateShipmentLineDto> Lines       { get; set; } = new();
    public List<CreateShipmentPackageDto> Packages { get; set; } = new();
}

public class UpdateShipmentDto
{
    public int WarehouseId { get; set; }

    public string? ShipToName         { get; set; }
    public string? ShipToContactName  { get; set; }
    public string? ShipToEmail        { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity         { get; set; }
    public string? ShipToState        { get; set; }
    public string? ShipToPostalCode   { get; set; }
    public string? ShipToCountry      { get; set; }
    public string? ShipToPhone        { get; set; }
    public string? ShipToTaxType      { get; set; }
    public string? ShipToTaxCountry   { get; set; }
    public string? ShipToTaxId        { get; set; }

    public string? ShipFromName         { get; set; }
    public string? ShipFromContactName  { get; set; }
    public string? ShipFromEmail        { get; set; }
    public string? ShipFromAddressLine1 { get; set; }
    public string? ShipFromAddressLine2 { get; set; }
    public string? ShipFromCity         { get; set; }
    public string? ShipFromState        { get; set; }
    public string? ShipFromPostalCode   { get; set; }
    public string? ShipFromCountry      { get; set; }
    public string? ShipFromPhone        { get; set; }

    public DateTime ShipDate        { get; set; } = DateTime.UtcNow.Date;
    public string?  Carrier         { get; set; }
    public string?  TrackingNumber  { get; set; }
    public bool     IsBlindShipment { get; set; }
    public int      OwnerId         { get; set; }

    public List<CreateShipmentLineDto> Lines       { get; set; } = new();
    public List<CreateShipmentPackageDto> Packages { get; set; } = new();
}
