using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/organization")]
[Authorize]
public class OrganizationController : ApiControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationController(IOrganizationService service) => _service = service;

    [Authorize(Policy = "organization.view")]
    [HttpGet]
    public async Task<IActionResult> GetProfile() =>
        Ok(await _service.GetProfileAsync(OrgId));

    [Authorize(Policy = "organization.edit")]
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateOrganizationDto dto) =>
        Ok(await _service.UpdateProfileAsync(OrgId, dto));
}
