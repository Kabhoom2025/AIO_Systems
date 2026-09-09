using FoodOrder.Application.DTOs.Payment;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentSvc;

    public PaymentController(IPaymentService paymentSvc)
        => _paymentSvc = paymentSvc;

    /// <summary>Creates a Razorpay order for the given POS order amount.</summary>
    [HttpPost("create-razorpay-order")]
    public async Task<IActionResult> CreateRazorpayOrder([FromBody] CreatePaymentOrderRequest request)
    {
        var result = await _paymentSvc.CreateRazorpayOrderAsync(
            request.Amount,
            $"pos_{request.PosOrderId}");

        return Ok(ApiResponse<RazorpayOrderResult>.SuccessResult(result, "Razorpay order created."));
    }

    /// <summary>Verifies the Razorpay payment signature after checkout completion.</summary>
    [HttpPost("verify-razorpay")]
    public async Task<IActionResult> VerifyRazorpayPayment([FromBody] VerifyPaymentRequest request)
    {
        var valid = await _paymentSvc.VerifyRazorpaySignatureAsync(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!valid)
            return BadRequest(ApiResponse<string>.FailureResult(
                "Payment verification failed. Invalid signature."));

        return Ok(ApiResponse<string>.SuccessResult("verified", "Payment verified successfully."));
    }
}
