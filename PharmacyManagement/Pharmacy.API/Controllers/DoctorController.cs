using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _service;

    public DoctorController(IDoctorService service) => _service = service;

    [Authorize(Policy = "doctors.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [AllowAnonymous]
    [HttpGet("public/org/{orgId}")]
    public async Task<IActionResult> GetPublicList(int orgId) =>
        Ok(await _service.GetPublicListAsync(orgId));

    [Authorize(Policy = "doctors.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "doctors.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateDoctorDto dto)
    {
        var result = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "doctors.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [Authorize(Policy = "doctors.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
