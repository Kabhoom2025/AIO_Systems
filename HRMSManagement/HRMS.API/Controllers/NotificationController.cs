using HRMS.API.Hubs;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HRMS.API.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationController : ApiControllerBase
{
    private readonly INotificationService _service;
    private readonly IHubContext<NotificationsHub> _hub;

    public NotificationController(INotificationService service, IHubContext<NotificationsHub> hub)
    {
        _service = service;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] int take = 50) =>
        Ok(await _service.GetForUserAsync(OrgId, UserId, take));

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount() =>
        Ok(new { count = await _service.GetUnreadCountAsync(OrgId, UserId) });

    [Authorize(Policy = "settings.edit")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
    {
        var result = await _service.CreateAsync(OrgId, dto);

        var target = dto.UserId.HasValue
            ? _hub.Clients.Group($"user-{dto.UserId}")
            : _hub.Clients.Group($"org-{OrgId}");
        await target.SendAsync("notification", result);

        return Ok(result);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _service.MarkReadAsync(id, UserId);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _service.MarkAllReadAsync(OrgId, UserId);
        return NoContent();
    }
}
