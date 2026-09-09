using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public class PatientController : ControllerBase
{
    private readonly IPatientService _service;

    public PatientController(IPatientService service) => _service = service;

    private int PatientId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Policy = "patient")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _service.GetByIdAsync(PatientId);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "patient")]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdatePatientProfileDto dto) =>
        Ok(await _service.UpdateProfileAsync(PatientId, dto));

    [Authorize(Policy = "patients.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [Authorize(Policy = "patients.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "patients.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreatePatientDto dto)
    {
        var result = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "patients.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [Authorize(Policy = "patients.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
