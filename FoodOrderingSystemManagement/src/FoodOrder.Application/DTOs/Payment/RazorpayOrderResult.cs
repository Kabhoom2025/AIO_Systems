namespace FoodOrder.Application.DTOs.Payment;

public class RazorpayOrderResult
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public long   Amount          { get; set; }  // in paise
    public string Currency        { get; set; } = "INR";
    public string Receipt         { get; set; } = string.Empty;
    public string KeyId           { get; set; } = string.Empty;
}
