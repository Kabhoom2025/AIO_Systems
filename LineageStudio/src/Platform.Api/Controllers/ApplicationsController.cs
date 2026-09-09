using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.Applications;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applications;

    public ApplicationsController(IApplicationService applications)
    {
        _applications = applications;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> List(CancellationToken ct)
        => Ok(await _applications.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationDto>> Get(Guid id, CancellationToken ct)
        => Ok(await _applications.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Create(CreateApplicationRequest request, CancellationToken ct)
    {
        var created = await _applications.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApplicationDto>> Update(Guid id, UpdateApplicationRequest request, CancellationToken ct)
        => Ok(await _applications.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _applications.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<ApplicationVersionDto>> Publish(Guid id, CancellationToken ct)
        => Ok(await _applications.PublishAsync(id, ct));

    [HttpPost("{id:guid}/unpublish")]
    public async Task<ActionResult<ApplicationDto>> Unpublish(Guid id, CancellationToken ct)
        => Ok(await _applications.UnpublishAsync(id, ct));

    [HttpGet("{id:guid}/versions")]
    public async Task<ActionResult<IReadOnlyList<ApplicationVersionDto>>> ListVersions(Guid id, CancellationToken ct)
        => Ok(await _applications.ListVersionsAsync(id, ct));
}
