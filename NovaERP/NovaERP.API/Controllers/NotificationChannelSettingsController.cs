using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/notification-channel-settings")]
[Authorize]
public class NotificationChannelSettingsController : ApiControllerBase
{
    private readonly INotificationChannelSettingsService _service;

    public NotificationChannelSettingsController(INotificationChannelSettingsService service) => _service = service;

    [Authorize(Policy = "settings.view")]
    [HttpGet]
    public async Task<IActionResult> Get() =>
        Ok(await _service.GetAsync(OrgId));

    [Authorize(Policy = "settings.edit")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateNotificationChannelSettingsDto dto) =>
        Ok(await _service.UpdateAsync(OrgId, dto));
}
