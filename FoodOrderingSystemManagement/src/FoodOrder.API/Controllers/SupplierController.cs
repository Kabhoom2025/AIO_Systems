using FoodOrder.Application.DTOs;
using FoodOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Authorize]
[ApiController]
[Route("api/suppliers")]
public class SupplierController(ISupplierService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier = await service.GetByIdAsync(id);
        return supplier == null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierRequest request)
    {
        try { return Ok(await service.UpdateAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    // ── Purchase Orders ───────────────────────────────────────────────────────
    [HttpGet("{supplierId:int}/purchase-orders")]
    public async Task<IActionResult> GetPurchaseOrders(int supplierId) =>
        Ok(await service.GetPurchaseOrdersAsync(supplierId));

    [HttpPost("purchase-orders")]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request)
    {
        var result = await service.CreatePurchaseOrderAsync(request);
        return CreatedAtAction(nameof(GetPurchaseOrders), new { supplierId = result.SupplierId }, result);
    }

    [HttpPatch("purchase-orders/{orderId:int}/status")]
    public async Task<IActionResult> UpdatePurchaseOrderStatus(int orderId, [FromBody] UpdatePurchaseOrderStatusRequest request)
    {
        try { return Ok(await service.UpdatePurchaseOrderStatusAsync(orderId, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Payments ──────────────────────────────────────────────────────────────
    [HttpGet("{supplierId:int}/payments")]
    public async Task<IActionResult> GetPayments(int supplierId) =>
        Ok(await service.GetPaymentsAsync(supplierId));

    [HttpPost("payments")]
    public async Task<IActionResult> CreatePayment([FromBody] CreateSupplierPaymentRequest request)
    {
        var result = await service.CreatePaymentAsync(request);
        return StatusCode(201, result);
    }
}
