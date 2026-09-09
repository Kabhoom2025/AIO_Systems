using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly ISaleService _service;

    public SaleController(ISaleService service) => _service = service;

    private int PatientId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Policy = "sales.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [Authorize(Policy = "sales.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "sales.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateSaleDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(orgId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

    [Authorize(Policy = "patient")]
    [HttpPost("online/org/{orgId}")]
    public async Task<IActionResult> CreateOnlineOrder(int orgId, [FromBody] CreateSaleDto dto)
    {
        try
        {
            var result = await _service.CreateOnlineOrderAsync(PatientId, orgId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

    [Authorize(Policy = "patient")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy() =>
        Ok(await _service.GetByPatientAsync(PatientId));

    [Authorize(Policy = "sales.view")]
    [HttpGet("online/org/{orgId}")]
    public async Task<IActionResult> GetOnlineOrders(int orgId) =>
        Ok(await _service.GetOnlineOrdersAsync(orgId));

    [Authorize(Policy = "sales.edit")]
    [HttpPost("{id}/fulfill")]
    public async Task<IActionResult> Fulfill(int id)
    {
        try
        {
            return Ok(await _service.FulfillOrderAsync(id));
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
