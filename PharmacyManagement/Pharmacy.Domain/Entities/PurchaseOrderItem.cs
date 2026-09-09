namespace Pharmacy.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public int     PurchaseOrderId  { get; set; }
    public int     MedicineId       { get; set; }
    public int     Quantity         { get; set; }
    public decimal UnitPrice        { get; set; }
    public int     ReceivedQuantity { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Medicine       Medicine     { get; set; } = null!;
}
