using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.Payment;

public class CreatePaymentOrderRequest
{
    [Required]
    public int PosOrderId { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";
}
