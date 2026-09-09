using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchController : ControllerBase
{
    private readonly IBranchService _service;

    public BranchController(IBranchService service) => _service = service;

    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetAll(int orgId) =>
        Ok(await _service.GetAllAsync(orgId));

    [AllowAnonymous]
    [HttpGet("public/org/{orgId}")]
    public async Task<IActionResult> GetPublicList(int orgId) =>
        Ok(await _service.GetPublicListAsync(orgId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("org/{orgId}")]
    public async Task<IActionResult> Create(int orgId, [FromBody] CreateBranchDto dto)
    {
        var result = await _service.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
