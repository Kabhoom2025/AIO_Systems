using System.ComponentModel.DataAnnotations;

namespace FoodOrder.Application.DTOs.Payment;

public class VerifyPaymentRequest
{
    [Required] public int    PosOrderId        { get; set; }
    [Required] public string RazorpayOrderId   { get; set; } = string.Empty;
    [Required] public string RazorpayPaymentId { get; set; } = string.Empty;
    [Required] public string RazorpaySignature { get; set; } = string.Empty;
}
