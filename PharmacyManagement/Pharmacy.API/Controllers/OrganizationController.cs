using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationController(IOrganizationService service) => _service = service;

    [AllowAnonymous]
    [HttpGet("public/default")]
    public async Task<IActionResult> GetPublicDefault()
    {
        var result = await _service.GetPublicDefaultAsync();
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "settings.view")]
    [HttpGet("{orgId}")]
    public async Task<IActionResult> GetById(int orgId)
    {
        var result = await _service.GetByIdAsync(orgId);
        return result == null ? NotFound() : Ok(result);
    }

    [Authorize(Policy = "settings.edit")]
    [HttpPut("{orgId}")]
    public async Task<IActionResult> Update(int orgId, [FromBody] UpdateOrganizationDto dto) =>
        Ok(await _service.UpdateAsync(orgId, dto));
}
