using FoodOrder.Domain.Enums;

namespace FoodOrder.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public int InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public StockTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }         // positive = stock in, negative = stock out
    public decimal StockBefore { get; set; }
    public decimal StockAfter { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public string? ReferenceNumber { get; set; }  // Invoice / PO number
    public string? Notes { get; set; }
    public int? RelatedTransactionId { get; set; } // links transfer source ↔ target
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
