using FoodOrder.Application.DTOs.Payment;

namespace FoodOrder.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<RazorpayOrderResult> CreateRazorpayOrderAsync(decimal amountInRupees, string receiptId);
    Task<bool> VerifyRazorpaySignatureAsync(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
}
