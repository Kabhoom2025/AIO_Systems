namespace FoodOrder.Application.DTOs;

// ── Supplier ──────────────────────────────────────────────────────────────────
public class SupplierDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? PaymentTerms { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public int PurchaseOrderCount { get; set; }
    public decimal TotalPaid { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSupplierRequest : CreateSupplierRequest
{
    public bool IsActive { get; set; } = true;
}

// ── Purchase Order ────────────────────────────────────────────────────────────
public class PurchaseOrderItemDTO
{
    public int Id { get; set; }
    public int InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? ReceivedQuantity { get; set; }
}

public class PurchaseOrderDTO
{
    public int Id { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemDTO> Items { get; set; } = [];
}

public class CreatePurchaseOrderItemRequest
{
    public int InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public int SupplierId { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = [];
}

public class UpdatePurchaseOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
}

// ── Supplier Payment ──────────────────────────────────────────────────────────
public class SupplierPaymentDTO
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int? PurchaseOrderId { get; set; }
    public string? PoNumber { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateSupplierPaymentRequest
{
    public int SupplierId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}
