namespace Pharmacy.Domain.Entities;

public class GoodsReceiptItem : BaseEntity
{
    public int       GoodsReceiptId      { get; set; }
    public int       PurchaseOrderItemId { get; set; }
    public int       MedicineId          { get; set; }
    public string    BatchNumber         { get; set; } = string.Empty;
    public DateTime  ExpiryDate          { get; set; }
    public DateTime? ManufacturingDate   { get; set; }
    public int       QuantityReceived    { get; set; }
    public decimal   PurchasePrice       { get; set; }

    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public Medicine     Medicine     { get; set; } = null!;
}
