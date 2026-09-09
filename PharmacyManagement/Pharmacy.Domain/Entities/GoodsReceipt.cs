namespace Pharmacy.Domain.Entities;

public class GoodsReceipt : BaseEntity
{
    public int      OrganizationId { get; set; }
    public int      PurchaseOrderId { get; set; }
    public string   ReceiptNumber  { get; set; } = string.Empty;
    public DateTime ReceivedDate   { get; set; } = DateTime.UtcNow;
    public string?  ReceivedBy     { get; set; }

    public Organization                    Organization  { get; set; } = null!;
    public PurchaseOrder                   PurchaseOrder { get; set; } = null!;
    public ICollection<GoodsReceiptItem>   Items         { get; set; } = new List<GoodsReceiptItem>();
}
