namespace FoodOrder.Application.DTOs.Order;

/// <summary>
/// Self-contained receipt returned after every successful order.
/// Designed to be consumed directly by any printer integration added in future
/// (USB, Thermal, ESC/POS, Network) without changing the business logic layer.
/// </summary>
public class ReceiptDto
{
    public string RestaurantName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? GSTNumber { get; set; }

    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? TableNumber { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; }
    public string CashierName { get; set; } = string.Empty;

    public List<ReceiptItemDto> Items { get; set; } = new();

    public decimal SubTotal { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal GrandTotal { get; set; }

    public string? UpiId { get; set; }
    public string FooterMessage { get; set; } = string.Empty;

    public string? CustomerName   { get; set; }
    public string? CustomerPhone2 { get; set; }
    public string? CustomerTier   { get; set; }
    public int     PointsEarned   { get; set; }
    public int     PointsRedeemed { get; set; }
    public int     NewPointsBalance { get; set; }
}
