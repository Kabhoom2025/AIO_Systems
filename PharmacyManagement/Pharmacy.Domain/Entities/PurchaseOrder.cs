namespace Pharmacy.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public int       OrganizationId       { get; set; }
    public int       SupplierId           { get; set; }
    public string    PoNumber             { get; set; } = string.Empty;
    public DateTime  OrderDate            { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string    Status               { get; set; } = "Draft"; // Draft | Sent | PartiallyReceived | Received | Cancelled
    public decimal   TotalAmount          { get; set; }
    public string?   Notes                { get; set; }

    public Organization                  Organization   { get; set; } = null!;
    public Supplier                      Supplier       { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items         { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<GoodsReceipt>      GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}
