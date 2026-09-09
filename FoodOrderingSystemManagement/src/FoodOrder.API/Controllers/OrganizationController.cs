using FoodOrder.Application.DTOs;
using FoodOrder.Application.DTOs.Settings;
using FoodOrder.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/organizations")]
public class OrganizationController(
    IOrganizationService service,
    ISettingsService settingsService,
    ILicenseService licenseService,
    IUserService userService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var org = await service.GetByIdAsync(id);
        return org == null ? NotFound() : Ok(org);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationRequest request)
    {
        try { return Ok(await service.UpdateAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{orgId:int}/users")]
    public async Task<IActionResult> GetOrgUsers(int orgId)
        => Ok(await service.GetOrgUsersAsync(orgId));

    [HttpPost("{orgId:int}/admins")]
    public async Task<IActionResult> CreateOrgAdmin(int orgId, [FromBody] CreateOrgAdminRequest request)
    {
        try { return Ok(await service.CreateOrgAdminAsync(orgId, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{orgId:int}/users/{userId:int}")]
    public async Task<IActionResult> DeleteOrgUser(int orgId, int userId)
    {
        await userService.DeleteUserAsync(userId, GetCurrentUserId());
        return NoContent();
    }

    [HttpGet("{orgId:int}/reports")]
    public async Task<IActionResult> GetOrgReport(int orgId)
    {
        try { return Ok(await service.GetOrgReportAsync(orgId)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{orgId:int}/settings")]
    public async Task<IActionResult> GetOrgSettings(int orgId)
        => Ok(await settingsService.GetOrgSettingsAsync(orgId));

    [HttpPut("{orgId:int}/settings")]
    public async Task<IActionResult> UpdateOrgSettings(int orgId, [FromBody] UpdateSettingsDto dto)
        => Ok(await settingsService.UpsertOrgSettingsAsync(orgId, dto));

    // ── License endpoints ─────────────────────────────────────────────────

    [HttpGet("licenses")]
    public async Task<IActionResult> GetAllLicenses()
        => Ok(await licenseService.GetAllAsync());

    [HttpGet("{orgId:int}/license")]
    public async Task<IActionResult> GetOrgLicense(int orgId)
    {
        var license = await licenseService.GetByOrgAsync(orgId);
        return license == null ? NotFound() : Ok(license);
    }

    [HttpPut("{orgId:int}/license")]
    public async Task<IActionResult> UpsertOrgLicense(int orgId, [FromBody] UpsertLicenseRequest request)
    {
        request.OrganizationId = orgId;
        return Ok(await licenseService.UpsertAsync(orgId, request));
    }

    [HttpDelete("{orgId:int}/license")]
    public async Task<IActionResult> DeleteOrgLicense(int orgId)
    {
        await licenseService.DeleteAsync(orgId);
        return NoContent();
    }
}
