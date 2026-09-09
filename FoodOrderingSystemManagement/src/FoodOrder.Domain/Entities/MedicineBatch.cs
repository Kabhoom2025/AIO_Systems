namespace FoodOrder.Domain.Entities;

public class MedicineBatch : BaseEntity
{
    public int      MedicineId          { get; set; }
    public string   BatchNumber         { get; set; } = string.Empty;
    public DateTime ExpiryDate          { get; set; }
    public DateTime? ManufacturingDate  { get; set; }
    public int      QuantityReceived    { get; set; }
    public int      CurrentQuantity     { get; set; }
    public decimal  PurchasePrice       { get; set; }

    public Medicine Medicine { get; set; } = null!;
}
