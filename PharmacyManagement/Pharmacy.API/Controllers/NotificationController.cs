using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service) => _service = service;

    [HttpGet("org/{orgId}")]
    public async Task<IActionResult> GetList(int orgId) =>
        Ok(await _service.GetListAsync(orgId));

    [HttpGet("unread-count/org/{orgId}")]
    public async Task<IActionResult> GetUnreadCount(int orgId) =>
        Ok(new { count = await _service.GetUnreadCountAsync(orgId) });

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _service.MarkReadAsync(id);
        return NoContent();
    }

    [HttpPost("org/{orgId}/read-all")]
    public async Task<IActionResult> MarkAllRead(int orgId)
    {
        await _service.MarkAllReadAsync(orgId);
        return NoContent();
    }
}
