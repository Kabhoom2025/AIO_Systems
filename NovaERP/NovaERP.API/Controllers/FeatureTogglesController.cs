using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/feature-toggles")]
[Authorize]
public class FeatureTogglesController : ApiControllerBase
{
    private readonly IFeatureToggleService _service;

    public FeatureTogglesController(IFeatureToggleService service) => _service = service;

    [Authorize(Policy = "settings.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(OrgId));

    [Authorize(Policy = "settings.edit")]
    [HttpPut("{moduleKey}")]
    public async Task<IActionResult> Update(string moduleKey, [FromBody] UpdateFeatureToggleDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, moduleKey, dto));
}
