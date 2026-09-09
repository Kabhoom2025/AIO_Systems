using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

/// <summary>Read-only reference list every authenticated user should be able to see (for
/// language pickers etc.) — no dedicated permission policy beyond [Authorize].</summary>
[Route("api/languages")]
[Authorize]
public class LanguageController : ApiControllerBase
{
    private readonly ILanguageService _service;

    public LanguageController(ILanguageService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());
}
