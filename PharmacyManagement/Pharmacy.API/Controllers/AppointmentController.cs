using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service) => _service = service;

    private int PatientId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int OrgId => int.Parse(User.FindFirst("organizationId")!.Value);

    [Authorize(Policy = "patient")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var result = await _service.CreateAsync(PatientId, OrgId, dto);
        return Ok(result);
    }

    [Authorize(Policy = "patient")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy() =>
        Ok(await _service.GetByPatientAsync(PatientId));

    [Authorize(Policy = "patient")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _service.CancelAsync(PatientId, id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "appointments.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [Authorize(Policy = "appointments.edit")]
    [HttpPost("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        try
        {
            return Ok(await _service.UpdateStatusAsync(id, dto.Status));
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
