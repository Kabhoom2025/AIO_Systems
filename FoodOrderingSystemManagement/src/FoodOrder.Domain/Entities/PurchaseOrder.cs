using FoodOrder.Domain.Enums;

namespace FoodOrder.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string PoNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = [];
    public ICollection<SupplierPayment> Payments { get; set; } = [];
}
