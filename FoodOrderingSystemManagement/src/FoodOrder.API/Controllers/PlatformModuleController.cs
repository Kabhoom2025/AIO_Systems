using FoodOrder.Application.DTOs.PlatformModule;
using FoodOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

/// <summary>Super-admin-only endpoint for managing platform modules and organization module assignments.</summary>
[Authorize]
[Route("api/platform-modules")]
public class PlatformModuleController : BaseApiController
{
    private readonly IPlatformModuleService _service;
    public PlatformModuleController(IPlatformModuleService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlatformModuleDto dto) =>
        Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlatformModuleDto dto) =>
        Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok();
    }

    [HttpGet("org-status")]
    public async Task<IActionResult> GetAllOrgStatus() =>
        Ok(await _service.GetAllOrgModuleStatusAsync());

    [HttpGet("org-status/{orgId:int}")]
    public async Task<IActionResult> GetOrgStatus(int orgId) =>
        Ok(await _service.GetOrgModuleStatusAsync(orgId));

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] OrgModuleAssignmentDto dto)
    {
        await _service.AssignModulesToOrgAsync(dto);
        return Ok();
    }
}
