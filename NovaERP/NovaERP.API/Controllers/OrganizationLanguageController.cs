using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/organizations/languages")]
[Authorize]
public class OrganizationLanguageController : ApiControllerBase
{
    private readonly IOrganizationLanguageService _service;

    public OrganizationLanguageController(IOrganizationLanguageService service) => _service = service;

    [Authorize(Policy = "settings.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "settings.edit")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateOrganizationLanguagesDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, dto));
}
