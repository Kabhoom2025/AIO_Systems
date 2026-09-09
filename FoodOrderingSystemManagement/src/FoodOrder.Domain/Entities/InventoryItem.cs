namespace FoodOrder.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty; // e.g. "kg", "liters", "pieces"
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Barcode { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }

    public ICollection<StockTransaction> Transactions { get; set; } = [];
}
