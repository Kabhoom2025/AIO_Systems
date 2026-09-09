using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/settings")]
[Authorize]
public class SettingsController : ApiControllerBase
{
    private readonly ISettingsService _service;

    public SettingsController(ISettingsService service) => _service = service;

    [Authorize(Policy = "settings.view")]
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _service.GetAsync(OrgId));

    [Authorize(Policy = "settings.edit")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateOrganizationSettingsDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, dto));
}
