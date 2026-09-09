using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

/// <summary>
/// Public (unauthenticated) endpoint for customers placing online/delivery orders.
/// </summary>
[Route("api/public/orders")]
[ApiController]
[AllowAnonymous]
public class PublicOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public PublicOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Place a delivery or dine-in order from the public menu page.
    /// No authentication required. Returns order number and confirmation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PublicOrderConfirmationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PublicOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(ApiResponse<object>.FailureResult("Order must contain at least one item."));

        if (string.IsNullOrWhiteSpace(dto.OrderType))
            dto.OrderType = "Delivery";

        if (dto.OrderType == "Delivery" && string.IsNullOrWhiteSpace(dto.DeliveryAddress))
            return BadRequest(ApiResponse<object>.FailureResult("Delivery address is required for delivery orders."));

        try
        {
            var confirmation = await _orderService.PlacePublicOrderAsync(dto);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<PublicOrderConfirmationDto>.SuccessResult(confirmation, "Order placed successfully!"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
    }
}
