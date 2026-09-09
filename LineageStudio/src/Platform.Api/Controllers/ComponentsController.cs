using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.UiBuilder;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/components")]
public class ComponentsController : ControllerBase
{
    private readonly IComponentService _components;

    public ComponentsController(IComponentService components)
    {
        _components = components;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ComponentDto>>> List(Guid applicationId, [FromQuery] Guid? screenId, CancellationToken ct)
        => Ok(await _components.ListAsync(applicationId, screenId, ct));

    [HttpGet("{componentId:guid}")]
    public async Task<ActionResult<ComponentDto>> Get(Guid applicationId, Guid componentId, CancellationToken ct)
        => Ok(await _components.GetAsync(applicationId, componentId, ct));

    [HttpPost]
    public async Task<ActionResult<ComponentDto>> Create(Guid applicationId, CreateComponentRequest request, CancellationToken ct)
    {
        var created = await _components.CreateAsync(applicationId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, componentId = created.Id }, created);
    }

    [HttpPut("{componentId:guid}")]
    public async Task<ActionResult<ComponentDto>> Update(Guid applicationId, Guid componentId, UpdateComponentRequest request, CancellationToken ct)
        => Ok(await _components.UpdateAsync(applicationId, componentId, request, ct));

    [HttpDelete("{componentId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid componentId, CancellationToken ct)
    {
        await _components.DeleteAsync(applicationId, componentId, ct);
        return NoContent();
    }
}
