using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.ApiDesigner;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceDefinitionService _services;

    public ServicesController(IServiceDefinitionService services)
    {
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> List(Guid applicationId, CancellationToken ct)
        => Ok(await _services.ListAsync(applicationId, ct));

    [HttpGet("{serviceId:guid}")]
    public async Task<ActionResult<ServiceDto>> Get(Guid applicationId, Guid serviceId, CancellationToken ct)
        => Ok(await _services.GetAsync(applicationId, serviceId, ct));

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create(Guid applicationId, CreateServiceRequest request, CancellationToken ct)
    {
        var created = await _services.CreateAsync(applicationId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, serviceId = created.Id }, created);
    }

    [HttpPut("{serviceId:guid}")]
    public async Task<ActionResult<ServiceDto>> Update(Guid applicationId, Guid serviceId, UpdateServiceRequest request, CancellationToken ct)
        => Ok(await _services.UpdateAsync(applicationId, serviceId, request, ct));

    [HttpDelete("{serviceId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid serviceId, CancellationToken ct)
    {
        await _services.DeleteAsync(applicationId, serviceId, ct);
        return NoContent();
    }
}
