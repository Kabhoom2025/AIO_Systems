using FoodOrder.Application.DTOs.Reservation;
using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly ITableReservationService _service;

    public ReservationController(ITableReservationService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var list = await _service.GetAllAsync(from, to);
        return Ok(ApiResponse<IReadOnlyList<ReservationDto>>.SuccessResult(list));
    }

    [HttpGet("upcoming")]
    [Authorize(Roles = "Admin,Cashier,Waiter")]
    public async Task<IActionResult> GetUpcoming()
    {
        var list = await _service.GetUpcomingAsync();
        return Ok(ApiResponse<IReadOnlyList<ReservationDto>>.SuccessResult(list));
    }

    [HttpGet("table/{tableId:int}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetByTable(int tableId)
    {
        var list = await _service.GetByTableAsync(tableId);
        return Ok(ApiResponse<IReadOnlyList<ReservationDto>>.SuccessResult(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _service.GetByIdAsync(id);
        if (r is null) return NotFound(ApiResponse<object>.FailureResult($"Reservation {id} not found."));
        return Ok(ApiResponse<ReservationDto>.SuccessResult(r));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest dto)
    {
        try
        {
            var r = await _service.CreateAsync(dto);
            return Ok(ApiResponse<ReservationDto>.SuccessResult(r));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationRequest dto)
    {
        var r = await _service.UpdateAsync(id, dto);
        if (r is null) return NotFound(ApiResponse<object>.FailureResult($"Reservation {id} not found."));
        return Ok(ApiResponse<ReservationDto>.SuccessResult(r));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Cashier")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservationStatusRequest dto)
    {
        try
        {
            var r = await _service.UpdateStatusAsync(id, dto.Status);
            if (r is null) return NotFound(ApiResponse<object>.FailureResult($"Reservation {id} not found."));
            return Ok(ApiResponse<ReservationDto>.SuccessResult(r));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResult(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.FailureResult($"Reservation {id} not found."));
        return Ok(ApiResponse<object>.SuccessResult(null!));
    }
}
