namespace FoodOrder.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? TableId { get; set; }
    public int? TableNumber { get; set; }
    public string? CustomerPhone    { get; set; }
    public string  OrderType        { get; set; } = "DineIn";
    public string? DeliveryAddress  { get; set; }
    public decimal DeliveryCharge   { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
