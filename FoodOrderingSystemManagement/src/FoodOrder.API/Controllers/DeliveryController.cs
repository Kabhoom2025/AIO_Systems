using FoodOrder.Application.DTOs.Delivery;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Route("api/delivery")]
[Authorize(Roles = "Admin,Cashier")]
public class DeliveryController : BaseApiController
{
    private readonly IDeliveryService _svc;
    public DeliveryController(IDeliveryService svc) => _svc = svc;

    // ── Dashboard ────────────────────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _svc.GetDashboardAsync(GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(stats));
    }

    // ── Drivers ──────────────────────────────────────────────────────────────

    [HttpGet("drivers")]
    public async Task<IActionResult> GetDrivers()
    {
        var list = await _svc.GetDriversAsync(GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("drivers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
    {
        var result = await _svc.CreateDriverAsync(dto, GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(result, "Driver created."));
    }

    [HttpPut("drivers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDto dto)
    {
        var result = await _svc.UpdateDriverAsync(id, dto);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Driver {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result, "Driver updated."));
    }

    [HttpPatch("drivers/{id:int}/location")]
    public async Task<IActionResult> UpdateDriverLocation(int id, [FromBody] UpdateDriverLocationDto dto)
    {
        var result = await _svc.UpdateDriverLocationAsync(id, dto);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Driver {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result, "Location updated."));
    }

    [HttpDelete("drivers/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDriver(int id)
    {
        await _svc.DeleteDriverAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null, "Driver deactivated."));
    }

    // ── Delivery Orders ──────────────────────────────────────────────────────

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] DateTime? date = null)
    {
        var list = await _svc.GetDeliveryOrdersAsync(GetCurrentOrganizationId(), date);
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var result = await _svc.GetDeliveryOrderAsync(id);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Delivery {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateDeliveryOrder([FromBody] CreateDeliveryOrderDto dto)
    {
        var result = await _svc.CreateDeliveryOrderAsync(dto);
        return Ok(ApiResponse<object>.SuccessResult(result, "Delivery order created."));
    }

    [HttpPatch("orders/{id:int}/assign")]
    public async Task<IActionResult> AssignDriver(int id, [FromBody] AssignDriverDto dto)
    {
        var result = await _svc.AssignDriverAsync(id, dto);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Delivery {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result, "Driver assigned."));
    }

    [HttpPatch("orders/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDeliveryStatusDto dto)
    {
        var result = await _svc.UpdateStatusAsync(id, dto);
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Delivery {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result, "Status updated."));
    }

    // ── Charge Slabs ─────────────────────────────────────────────────────────

    [HttpGet("charges")]
    public async Task<IActionResult> GetCharges()
    {
        var list = await _svc.GetChargeSlabsAsync(GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("charges")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddChargeSlab([FromBody] UpsertDeliveryChargeSlabDto dto)
    {
        var result = await _svc.UpsertChargeSlabAsync(dto, GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(result, "Charge slab added."));
    }

    [HttpDelete("charges/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteChargeSlab(int id)
    {
        await _svc.DeleteChargeSlabAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null, "Charge slab deleted."));
    }

    // ── Third Party Dispatch ─────────────────────────────────────────────────

    [HttpPost("orders/{id:int}/dispatch")]
    public async Task<IActionResult> DispatchToThirdParty(int id, [FromBody] DispatchToProviderDto dto)
    {
        var result = await _svc.DispatchToThirdPartyAsync(id, dto.Provider, GetCurrentOrganizationId());
        if (result is null) return NotFound(ApiResponse<object>.FailureResult($"Delivery {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(result, $"Dispatched to {dto.Provider}."));
    }

    // ── Third Party Config ───────────────────────────────────────────────────

    [HttpGet("third-party")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetThirdPartyConfigs()
    {
        var list = await _svc.GetThirdPartyConfigsAsync(GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(list));
    }

    [HttpPost("third-party")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpsertThirdPartyConfig([FromBody] UpsertThirdPartyConfigDto dto)
    {
        var result = await _svc.UpsertThirdPartyConfigAsync(dto, GetCurrentOrganizationId());
        return Ok(ApiResponse<object>.SuccessResult(result, "Config saved."));
    }
}
