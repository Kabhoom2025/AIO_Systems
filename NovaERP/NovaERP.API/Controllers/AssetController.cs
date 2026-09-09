using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/assets")]
[Authorize]
public class AssetController : ApiControllerBase
{
    private readonly IAssetService _service;

    public AssetController(IAssetService service) => _service = service;

    [Authorize(Policy = "assets.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "assets.view")]
    [HttpGet("{id}")]
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
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, id, dto));

    [Authorize(Policy = "assets.delete")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(OrgId, id);
        return NoContent();
    }

    [Authorize(Policy = "assets.edit")]
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignAssetDto dto) =>
        Ok(await _service.AssignAsync(OrgId, id, dto));

    [Authorize(Policy = "assets.edit")]
    [HttpPost("{id}/unassign")]
    public async Task<IActionResult> Unassign(int id) =>
        Ok(await _service.UnassignAsync(OrgId, id));

    [Authorize(Policy = "assets.edit")]
    [HttpPost("{id}/retire")]
    public async Task<IActionResult> Retire(int id) =>
        Ok(await _service.RetireAsync(OrgId, id));
}
