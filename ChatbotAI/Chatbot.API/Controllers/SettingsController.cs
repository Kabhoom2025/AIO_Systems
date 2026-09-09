using System.Security.Claims;
using Chatbot.Application.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.API.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public class SettingsController(IUserSettingsService userSettingsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserSettingsDto>> Get(CancellationToken cancellationToken)
    {
        var result = await userSettingsService.GetAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<UserSettingsDto>> Update(UpdateUserSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await userSettingsService.UpdateAsync(GetUserId(), request, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
