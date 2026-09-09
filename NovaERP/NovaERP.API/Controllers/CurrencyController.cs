using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Currencies are mostly system-wide reference data, so create/update are gated on
/// the "settings" policies (same as SeetingsController) rather than a dedicated permission.</summary>
[Route("api/currencies")]
[Authorize]
public class CurrencyController : ApiControllerBase
{
    private readonly ICurrencyService _service;

    public CurrencyController(ICurrencyService service) => _service = service;

    [Authorize(Policy = "settings.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [Authorize(Policy = "settings.create")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto dto) =>
        Ok(await _service.CreateAsync(dto));

    [Authorize(Policy = "settings.edit")]
    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateCurrencyDto dto) =>
        Ok(await _service.UpdateAsync(code, dto));
}
