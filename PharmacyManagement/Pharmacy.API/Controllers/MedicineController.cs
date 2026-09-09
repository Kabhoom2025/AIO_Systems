using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/medicines")]
[Authorize]
public class MedicineController : ControllerBase
{
    private readonly IMedicineService _service;

    public MedicineController(IMedicineService service) => _service = service;

    [Authorize(Policy = "medicines.view")]
    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [HttpGet("public/org/{orgId}")]
    public async Task<IActionResult> GetPublicList(int orgId) =>
        Ok(await _service.GetPublicListAsync(orgId));

    [Authorize(Policy = "medicines.view")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "medicines.create")]
    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateMedicineDto dto)
    {
        var result = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "medicines.edit")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [Authorize(Policy = "medicines.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [Authorize(Policy = "medicines.edit")]
    [HttpPost("{medicineId}/batches")]
    public async Task<IActionResult> AddBatch(int medicineId, [FromBody] AddBatchDto dto) =>
        Ok(await _service.AddBatchAsync(medicineId, dto));

    [Authorize(Policy = "medicines.view")]
    [HttpGet("barcode/{code}/org/{orgId}")]
    public async Task<IActionResult> GetByCode(string code, int orgId)
    {
        var result = await _service.GetByCodeAsync(orgId, code);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "medicines.view")]
    [HttpGet("expiry-alerts/org/{orgId}")]
    public async Task<IActionResult> ExpiryAlerts(int orgId, [FromQuery] int days = 90) =>
        Ok(await _service.GetExpiryAlertsAsync(orgId, days));
}
