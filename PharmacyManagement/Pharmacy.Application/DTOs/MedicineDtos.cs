namespace Pharmacy.Application.DTOs;

public class MedicineDto
{
    public int     Id            { get; set; }
    public string  Name          { get; set; } = string.Empty;
    public string  GenericName   { get; set; } = string.Empty;
    public string  Category      { get; set; } = string.Empty;
    public string  DrugSchedule  { get; set; } = string.Empty;
    public string  PackType      { get; set; } = string.Empty;
    public string  PackSize      { get; set; } = string.Empty;
    public string  Unit          { get; set; } = string.Empty;
    public string  Manufacturer  { get; set; } = string.Empty;
    public string? HsnCode       { get; set; }
    public string  Sku           { get; set; } = string.Empty;
    public string  Barcode       { get; set; } = string.Empty;
    public decimal MRP           { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal GstPercent    { get; set; }
    public string? RackLocation  { get; set; }
    public int     ReorderLevel  { get; set; }
    public bool    IsActive      { get; set; }
    public int     TotalStock    { get; set; }
    public List<MedicineBatchDto> Batches { get; set; } = new();
}

public class MedicineBatchDto
{
    public int      Id                 { get; set; }
    public int      MedicineId         { get; set; }
    public string   BatchNumber        { get; set; } = string.Empty;
    public DateTime ExpiryDate         { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public int      QuantityReceived   { get; set; }
    public int      CurrentQuantity    { get; set; }
    public decimal  PurchasePrice      { get; set; }
    public int      DaysToExpiry       { get; set; }
    public string   ExpiryStatus       { get; set; } = string.Empty;
}

public class CreateMedicineDto
{
    public string  Name          { get; set; } = string.Empty;
    public string  GenericName   { get; set; } = string.Empty;
    public string  Category      { get; set; } = string.Empty;
    public string  DrugSchedule  { get; set; } = "OTC";
    public string  PackType      { get; set; } = string.Empty;
    public string  PackSize      { get; set; } = string.Empty;
    public string  Unit          { get; set; } = string.Empty;
    public string  Manufacturer  { get; set; } = string.Empty;
    public string? HsnCode       { get; set; }
    public string? Sku           { get; set; }
    public string? Barcode       { get; set; }
    public decimal MRP           { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal GstPercent    { get; set; } = 12;
    public string? RackLocation  { get; set; }
    public int     ReorderLevel  { get; set; } = 10;
}

public class UpdateMedicineDto : CreateMedicineDto
{
    public bool IsActive { get; set; } = true;
}

public class AddBatchDto
{
    public string   BatchNumber        { get; set; } = string.Empty;
    public DateTime ExpiryDate         { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public int      Quantity           { get; set; }
    public decimal  PurchasePrice      { get; set; }
}

public class PublicMedicineDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public string  GenericName { get; set; } = string.Empty;
    public string  Category    { get; set; } = string.Empty;
    public decimal MRP         { get; set; }
    public string  Unit        { get; set; } = string.Empty;
    public string  PackSize    { get; set; } = string.Empty;
    public int     TotalStock  { get; set; }
}

public class ExpiryAlertDto
{
    public int      MedicineId   { get; set; }
    public string   MedicineName { get; set; } = string.Empty;
    public string   GenericName  { get; set; } = string.Empty;
    public int      BatchId      { get; set; }
    public string   BatchNumber  { get; set; } = string.Empty;
    public DateTime ExpiryDate   { get; set; }
    public int      DaysToExpiry { get; set; }
    public int      Quantity     { get; set; }
    public string   ExpiryStatus { get; set; } = string.Empty;
    public string?  RackLocation { get; set; }
}
