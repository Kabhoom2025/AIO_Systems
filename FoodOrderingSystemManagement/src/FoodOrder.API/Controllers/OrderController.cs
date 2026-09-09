using FluentValidation;
using FoodOrder.API.Hubs;
using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FoodOrder.API.Controllers;

public record TransferTableRequest(int NewTableId);
public record GenerateBillRequest(decimal? Discount);
public record KitchenRejectRequest(string? Reason);
public record RejectItemsRequest(List<int> FoodItemIds, string? Reason);

[Route("api/[controller]")]
[Authorize]
public class OrderController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IHubContext<OrderHub> _hub;

    public OrderController(IOrderService orderService, IValidator<CreateOrderDto> createValidator, IHubContext<OrderHub> hub)
    {
        _orderService = orderService;
        _createValidator = createValidator;
        _hub = hub;
    }

    /// <summary>
    /// Place a new order. Validates stock, calculates bill, deducts quantities, and
    /// returns a fully populated receipt DTO ready for any printer integration.
    /// Accessible by Admin and Cashier.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReceiptDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var error = await ValidateAsync(_createValidator, dto);
        if (error != null) return error;

        var receipt = await _orderService.CreateOrderAsync(dto, GetCurrentUserId());

        await _hub.Clients.All.SendAsync("OrderCreated", new
        {
            orderId     = receipt.OrderId,
            orderNumber = receipt.OrderNumber,
            tableNumber = dto.TableId,
            items       = receipt.Items.Select(i => new { name = i.ItemName, qty = i.Quantity, addOnNotes = (string?)null }),
        });

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<ReceiptDto>.SuccessResult(receipt, "Order placed successfully."));
    }

    /// <summary>Get all pending (open) dine-in orders. Accessible by Admin and Cashier.</summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingOrders()
    {
        var orders = await _orderService.GetPendingOrdersAsync();
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.SuccessResult(orders));
    }

    /// <summary>Add items to an open (Pending) dine-in order.</summary>
    [HttpPost("{id:int}/add-items")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItems(int id, [FromBody] AddItemsToOrderDto dto)
    {
        var order = await _orderService.AddItemsToOrderAsync(id, dto);

        await _hub.Clients.All.SendAsync("OrderCreated", new
        {
            orderId     = order.Id,
            orderNumber = order.OrderNumber,
            tableNumber = order.TableNumber,
            items       = order.Items?.Select(i => new { name = i.ItemName, qty = i.Quantity, addOnNotes = i.AddOnNotes }),
        });

        return Ok(ApiResponse<OrderDto>.SuccessResult(order, "Items added to order."));
    }

    /// <summary>Finalize a dine-in order — optionally apply a final discount, set to BillPending, return receipt.</summary>
    [HttpPatch("{id:int}/generate-bill")]
    [ProducesResponseType(typeof(ApiResponse<ReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateBill(int id, [FromBody] GenerateBillRequest? request = null)
    {
        var receipt = await _orderService.GenerateBillAsync(id, request?.Discount);

        await _hub.Clients.All.SendAsync("OrderStatusChanged", new
        {
            orderId     = receipt.OrderId,
            orderNumber = receipt.OrderNumber,
            status      = "BillPending",
            tableNumber = receipt.TableNumber,
        });

        return Ok(ApiResponse<ReceiptDto>.SuccessResult(receipt, "Bill generated successfully."));
    }

    /// <summary>Confirm payment for a BillPending order — sets status to Completed, freeing the table.</summary>
    [HttpPatch("{id:int}/confirm-payment")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        await _orderService.ConfirmPaymentAsync(id);
        var order = await _orderService.GetOrderByIdAsync(id);

        await _hub.Clients.All.SendAsync("OrderStatusChanged", new
        {
            orderId     = order.Id,
            orderNumber = order.OrderNumber,
            status      = "Completed",
            tableNumber = order.TableNumber,
        });

        return Ok(ApiResponse<object>.SuccessResult(new { }, "Payment confirmed. Table is now available."));
    }

    /// <summary>
    /// Get orders filtered by the caller's role:
    ///   - Waiter  → only their own orders (CashierId == current user)
    ///   - Cashier → all orders (sees all waiters + cashiers)
    ///   - Admin   → all orders (across every branch they can see) — pass
    ///     ?branchId= to narrow to one specific branch instead of all combined.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders([FromQuery] int? branchId)
    {
        var role   = GetCurrentUserRole();
        var userId = GetCurrentUserId();

        IReadOnlyList<OrderDto> orders = role == "Waiter"
            ? await _orderService.GetOrdersByUserAsync(userId)
            : await _orderService.GetAllOrdersAsync(branchId);

        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.SuccessResult(orders));
    }

    /// <summary>Get a single order with all line items by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(ApiResponse<OrderDto>.SuccessResult(order));
    }

    /// <summary>Transfer an order to a different table.</summary>
    [HttpPatch("{id:int}/transfer-table")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferTable(int id, [FromBody] TransferTableRequest request)
    {
        await _orderService.TransferTableAsync(id, request.NewTableId);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order transferred to new table successfully."));
    }

    /// <summary>Remove table assignment — move order to Takeaway.</summary>
    [HttpPatch("{id:int}/transfer-takeaway")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TransferToTakeaway(int id)
    {
        await _orderService.TransferToTakeawayAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order moved to Takeaway."));
    }

    /// <summary>
    /// Cancel an order within the 1-minute window. Restores stock for all line items.
    /// Admin only — Waiters and Cashiers cannot cancel orders.
    /// </summary>
    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        await _orderService.CancelOrderAsync(id, GetCurrentUserId());
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order cancelled successfully. Stock has been restored."));
    }

    /// <summary>Reopen a cancelled order — restores it to Pending and re-deducts stock. Admin only.</summary>
    [HttpPatch("{id:int}/reopen")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReopenOrder(int id)
    {
        await _orderService.ReopenOrderAsync(id);

        await _hub.Clients.All.SendAsync("OrderStatusChanged", new
        {
            orderId = id,
            status  = "Pending",
        });

        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order reopened and restored to Pending."));
    }

    /// <summary>Permanently delete a Completed or Cancelled order. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderService.DeleteOrderAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order permanently deleted."));
    }

    /// <summary>Kitchen rejects specific items — removes them, restores stock, recalculates totals. If all items rejected, cancels the order.</summary>
    [HttpPatch("{id:int}/reject-items")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> RejectItems(int id, [FromBody] RejectItemsRequest req)
    {
        var (orderCancelled, removedNames) = await _orderService.RejectOrderItemsAsync(id, req.FoodItemIds, req.Reason?.Trim());
        var order = orderCancelled ? null : await _orderService.GetOrderByIdAsync(id);

        await _hub.Clients.All.SendAsync("OrderItemsRejected", new
        {
            orderId        = id,
            orderNumber    = order?.OrderNumber ?? "",
            tableNumber    = order?.TableNumber,
            orderCancelled,
            rejectedItems  = removedNames,
            reason         = req.Reason?.Trim() ?? "",
        });

        var msg = orderCancelled
            ? "All items rejected — order cancelled and stock restored."
            : $"{removedNames.Count} item(s) removed. Order totals recalculated.";

        return Ok(ApiResponse<object>.SuccessResult(new { orderCancelled, removedItems = removedNames }, msg));
    }

    /// <summary>Kitchen rejects an order — sets Cancelled, restores stock, broadcasts to POS. Accessible by Admin and Cashier.</summary>
    [HttpPatch("{id:int}/kitchen-reject")]
    [Authorize(Roles = "Admin,Cashier")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> KitchenReject(int id, [FromBody] KitchenRejectRequest? req = null)
    {
        var reason = req?.Reason?.Trim();
        await _orderService.KitchenRejectOrderAsync(id, reason);

        var order = await _orderService.GetOrderByIdAsync(id);
        await _hub.Clients.All.SendAsync("OrderRejectedByKitchen", new
        {
            orderId     = order.Id,
            orderNumber = order.OrderNumber,
            tableNumber = order.TableNumber,
            reason      = reason ?? "Rejected by kitchen",
        });

        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order rejected. Stock restored and cashier notified."));
    }

    [HttpPatch("{id:int}/mark-ready")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> MarkReady(int id)
    {
        var order = await _orderService.MarkOrderReadyAsync(id);
        await _hub.Clients.All.SendAsync("OrderStatusChanged", new
        {
            orderId     = order.Id,
            orderNumber = order.OrderNumber,
            status      = "KitchenReady",
            tableNumber = order.TableNumber,
        });
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order marked as ready. Kitchen notification sent."));
    }

    [HttpPatch("{id:int}/mark-served")]
    [Authorize(Roles = "Admin,Cashier,Waiter")]
    public async Task<IActionResult> MarkServed(int id)
    {
        var order = await _orderService.MarkOrderServedAsync(id);
        await _hub.Clients.All.SendAsync("OrderStatusChanged", new
        {
            orderId     = order.Id,
            orderNumber = order.OrderNumber,
            status      = "Pending",
            tableNumber = order.TableNumber,
        });
        return Ok(ApiResponse<object>.SuccessResult(new { }, "Order picked up — status reset to pending."));
    }
}
