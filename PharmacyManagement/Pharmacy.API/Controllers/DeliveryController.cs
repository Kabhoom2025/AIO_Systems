using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _service;

    public DeliveryController(IDeliveryService service) => _service = service;

    [Authorize(Policy = "deliveries.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [Authorize(Policy = "deliveries.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "deliveries.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateDeliveryDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(orgId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "deliveries.edit")]
    [HttpPatch("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignDeliveryStaffDto dto) =>
        await HandleTransition(() => _service.AssignStaffAsync(id, dto));

    [Authorize(Policy = "deliveries.edit")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateDeliveryStatusDto dto) =>
        await HandleTransition(() => _service.UpdateStatusAsync(id, dto));

    [Authorize(Policy = "deliveries.edit")]
    [HttpPatch("{id}/verify-otp")]
    public async Task<IActionResult> VerifyOtp(int id, [FromBody] VerifyDeliveryOtpDto dto) =>
        await HandleTransition(() => _service.VerifyOtpAsync(id, dto));

    private async Task<IActionResult> HandleTransition(Func<Task<DeliveryDto>> action)
    {
        try
        {
            return Ok(await action());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
