namespace Pharmacy.Application.DTOs;

public class SaleDto
{
    public int      Id             { get; set; }
    public string   InvoiceNumber  { get; set; } = string.Empty;
    public DateTime SaleDate       { get; set; }
    public int      BranchId       { get; set; }
    public string   BranchName     { get; set; } = string.Empty;
    public int?     CustomerId     { get; set; }
    public string?  CustomerName   { get; set; }
    public int?     PatientId      { get; set; }
    public string?  PatientName    { get; set; }
    public string?  PatientPhone   { get; set; }
    public string   Channel        { get; set; } = "POS";
    public decimal  Subtotal       { get; set; }
    public decimal  DiscountAmount { get; set; }
    public decimal  TaxAmount      { get; set; }
    public decimal  TotalAmount    { get; set; }
    public string   PaymentMethod  { get; set; } = string.Empty;
    public string   Status         { get; set; } = string.Empty;
    public List<SaleItemDto> Items { get; set; } = new();
}

public class SaleItemDto
{
    public int     Id           { get; set; }
    public int     MedicineId   { get; set; }
    public string  MedicineName { get; set; } = string.Empty;
    public string  BatchNumber  { get; set; } = string.Empty;
    public int     Quantity     { get; set; }
    public decimal UnitPrice    { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GstPercent   { get; set; }
    public decimal LineTotal    { get; set; }
}

public class CreateSaleDto
{
    public int     BranchId      { get; set; }
    public int?    CustomerId    { get; set; }
    public string  PaymentMethod { get; set; } = "Cash";
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    public int     MedicineId     { get; set; }
    public int     Quantity       { get; set; }
    public decimal DiscountAmount { get; set; }
}
