namespace FoodOrder.Application.DTOs.Order;

public class PublicOrderDto
{
    /// <summary>Which branch's menu this order is placed against — required since this endpoint is anonymous.</summary>
    public int     BranchId        { get; set; }
    public string  CustomerName    { get; set; } = string.Empty;
    public string? CustomerPhone   { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Notes           { get; set; }
    public int?    TableId         { get; set; }
    public string  OrderType       { get; set; } = "Delivery";
    public decimal DeliveryCharge  { get; set; } = 0;
    public List<PublicOrderItemDto> Items { get; set; } = new();
}

public class PublicOrderItemDto
{
    public int       FoodItemId { get; set; }
    public int       Quantity   { get; set; }
    public List<int> AddOnIds   { get; set; } = new();
}

public class PublicOrderConfirmationDto
{
    public string  OrderNumber    { get; set; } = string.Empty;
    public int     OrderId        { get; set; }
    public decimal GrandTotal     { get; set; }
    public string  OrderType      { get; set; } = string.Empty;
    public string  CustomerName   { get; set; } = string.Empty;
    public string? EstimatedTime  { get; set; }
}
