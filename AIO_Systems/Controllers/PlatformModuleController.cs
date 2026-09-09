using AIO_Systems.DTOs.PlatformModule;
using AIO_Systems.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIO_Systems.Controllers;

[Authorize]
[Route("api/platform-modules")]
public class PlatformModuleController(IPlatformModuleService platformModuleService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var modules = await platformModuleService.GetAllAsync();
        return Ok(modules);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var module = await platformModuleService.GetByIdAsync(id);
        return Ok(module);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlatformModuleDto dto)
    {
        var module = await platformModuleService.CreateAsync(dto);
        return Ok(module);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlatformModuleDto dto)
    {
        var module = await platformModuleService.UpdateAsync(id, dto);
        return Ok(module);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await platformModuleService.DeleteAsync(id);
        return Ok();
    }

    [HttpGet("org-status")]
    public async Task<IActionResult> GetAllOrgModuleStatus()
    {
        var status = await platformModuleService.GetAllOrgModuleStatusAsync();
        return Ok(status);
    }

    [HttpGet("org-status/{orgId:int}")]
    public async Task<IActionResult> GetOrgModuleStatus(int orgId)
    {
        var status = await platformModuleService.GetOrgModuleStatusAsync(orgId);
        return Ok(status);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignModulesToOrg([FromBody] OrgModuleAssignmentDto dto)
    {
        await platformModuleService.AssignModulesToOrgAsync(dto);
        return Ok();
    }
}
