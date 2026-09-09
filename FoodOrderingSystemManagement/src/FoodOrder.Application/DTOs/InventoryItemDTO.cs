namespace FoodOrder.Application.DTOs;

public class InventoryItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
    public string? Barcode { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class CreateInventoryItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public string? Barcode { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateInventoryItemRequest : CreateInventoryItemRequest
{
    public bool IsActive { get; set; } = true;
}

public class StockAdjustmentRequest
{
    public decimal Quantity { get; set; } // positive = add, negative = remove/consume
    public string? Note { get; set; }
}

public class PurchaseStockRequest
{
    public decimal Quantity { get; set; }
    public string? Supplier { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class WasteStockRequest
{
    public decimal Quantity { get; set; }  // always positive — service makes it negative
    public string? WasteReason { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
}

public class TransferStockRequest
{
    public int TargetInventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class StockTransactionDTO
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal StockBefore { get; set; }
    public decimal StockAfter { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Supplier { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? RelatedTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
