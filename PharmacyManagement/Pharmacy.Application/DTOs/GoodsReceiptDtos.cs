namespace Pharmacy.Application.DTOs;

public class GoodsReceiptDto
{
    public int      Id              { get; set; }
    public int      PurchaseOrderId { get; set; }
    public string   PoNumber        { get; set; } = string.Empty;
    public string   ReceiptNumber   { get; set; } = string.Empty;
    public DateTime ReceivedDate    { get; set; }
    public string?  ReceivedBy      { get; set; }
    public List<GoodsReceiptItemDto> Items { get; set; } = new();
}

public class GoodsReceiptItemDto
{
    public int       Id                  { get; set; }
    public int       PurchaseOrderItemId { get; set; }
    public int       MedicineId          { get; set; }
    public string    MedicineName        { get; set; } = string.Empty;
    public string    BatchNumber         { get; set; } = string.Empty;
    public DateTime  ExpiryDate          { get; set; }
    public DateTime? ManufacturingDate   { get; set; }
    public int       QuantityReceived    { get; set; }
    public decimal   PurchasePrice       { get; set; }
}

public class CreateGoodsReceiptDto
{
    public int     PurchaseOrderId { get; set; }
    public string? ReceivedBy      { get; set; }
    public List<CreateGoodsReceiptItemDto> Items { get; set; } = new();
}

public class CreateGoodsReceiptItemDto
{
    public int       PurchaseOrderItemId { get; set; }
    public int       MedicineId          { get; set; }
    public string    BatchNumber         { get; set; } = string.Empty;
    public DateTime  ExpiryDate          { get; set; }
    public DateTime? ManufacturingDate   { get; set; }
    public int       QuantityReceived    { get; set; }
    public decimal   PurchasePrice       { get; set; }
}
