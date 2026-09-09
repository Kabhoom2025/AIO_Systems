using AIO_Systems.DTOs.Organization;
using AIO_Systems.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIO_Systems.Controllers;

[Authorize(Roles = "SuperAdmin")]
[Route("api/organizations")]
public class OrganizationController(IOrganizationService organizationService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orgs = await organizationService.GetAllAsync();
        return Ok(orgs);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var org = await organizationService.GetByIdAsync(id);
        if (org == null) return NotFound();
        return Ok(org);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request)
    {
        var org = await organizationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = org.Id }, org);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationRequest request)
    {
        var org = await organizationService.UpdateAsync(id, request);
        return Ok(org);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await organizationService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{orgId:int}/users")]
    public async Task<IActionResult> GetOrgUsers(int orgId)
    {
        var users = await organizationService.GetOrgUsersAsync(orgId);
        return Ok(users);
    }

    [HttpPost("{orgId:int}/admins")]
    public async Task<IActionResult> CreateOrgAdmin(int orgId, [FromBody] CreateOrgAdminRequest request)
    {
        var user = await organizationService.CreateOrgAdminAsync(orgId, request);
        return Ok(user);
    }

    [HttpGet("licenses")]
    public async Task<IActionResult> GetAllLicenses()
    {
        var licenses = await organizationService.GetAllLicensesAsync();
        return Ok(licenses);
    }

    [HttpGet("{orgId:int}/license")]
    public async Task<IActionResult> GetLicense(int orgId)
    {
        var license = await organizationService.GetLicenseByOrgAsync(orgId);
        if (license == null) return NotFound();
        return Ok(license);
    }

    [HttpPut("{orgId:int}/license")]
    public async Task<IActionResult> UpsertLicense(int orgId, [FromBody] UpsertLicenseRequest request)
    {
        request.OrganizationId = orgId;
        var license = await organizationService.UpsertLicenseAsync(orgId, request);
        return Ok(license);
    }

    [HttpDelete("{orgId:int}/license")]
    public async Task<IActionResult> DeleteLicense(int orgId)
    {
        await organizationService.DeleteLicenseAsync(orgId);
        return NoContent();
    }
}
