using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/permissions")]
[Authorize]
public class PermissionController : ApiControllerBase
{
    private readonly IPermissionService _service;

    public PermissionController(IPermissionService service) => _service = service;

    [Authorize(Policy = "roles.view")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());
}
