using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.Application.UiBuilder;

namespace Platform.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/applications/{applicationId:guid}/screens")]
public class ScreensController : ControllerBase
{
    private readonly IScreenService _screens;

    public ScreensController(IScreenService screens)
    {
        _screens = screens;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ScreenDto>>> List(Guid applicationId, CancellationToken ct)
        => Ok(await _screens.ListAsync(applicationId, ct));

    [HttpGet("{screenId:guid}")]
    public async Task<ActionResult<ScreenDto>> Get(Guid applicationId, Guid screenId, CancellationToken ct)
        => Ok(await _screens.GetAsync(applicationId, screenId, ct));

    [HttpPost]
    public async Task<ActionResult<ScreenDto>> Create(Guid applicationId, CreateScreenRequest request, CancellationToken ct)
    {
        var created = await _screens.CreateAsync(applicationId, request, ct);
        return CreatedAtAction(nameof(Get), new { applicationId, screenId = created.Id }, created);
    }

    [HttpPut("{screenId:guid}")]
    public async Task<ActionResult<ScreenDto>> Update(Guid applicationId, Guid screenId, UpdateScreenRequest request, CancellationToken ct)
        => Ok(await _screens.UpdateAsync(applicationId, screenId, request, ct));

    [HttpDelete("{screenId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid screenId, CancellationToken ct)
    {
        await _screens.DeleteAsync(applicationId, screenId, ct);
        return NoContent();
    }
}
