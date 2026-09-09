using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Route("api/assets")]
[Authorize]
public class AssetController : ApiControllerBase
{
    private readonly IAssetService _service;

    public AssetController(IAssetService service) => _service = service;

    [Authorize(Policy = "assets.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? status) =>
        Ok(await _service.GetAllAsync(OrgId, category, status));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        if (EmployeeId == null)
            return BadRequest("No employee profile linked to this user.");
        return Ok(await _service.GetMyAssetsAsync(OrgId, EmployeeId.Value));
    }

    [Authorize(Policy = "assets.view")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(OrgId, id));

    [Authorize(Policy = "assets.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "assets.edit")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "assets.delete")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "assets.edit")]
    [HttpPost("{id:int}/allocate")]
    public async Task<IActionResult> Allocate(int id, [FromBody] AllocateAssetDto dto) =>
        Ok(await _service.AllocateAsync(OrgId, id, UserId, dto));

    [Authorize(Policy = "assets.edit")]
    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(int id, [FromBody] ReturnAssetDto dto) =>
        Ok(await _service.ReturnAsync(OrgId, id, dto));
}
