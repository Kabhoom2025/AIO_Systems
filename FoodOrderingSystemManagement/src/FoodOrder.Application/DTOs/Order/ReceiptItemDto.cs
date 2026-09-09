namespace FoodOrder.Application.DTOs.Order;

public class ReceiptItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? AddOnNotes { get; set; }
}
