using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NovaERP.API.Hubs;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;

namespace NovaERP.API.Controllers;

[Route("api/notifications")]
[Authorize]
public class NotificationController : ApiControllerBase
{
    private readonly INotificationService _service;
    private readonly IHubContext<NotificationsHub> _hub;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushSender _pushSender;
    private readonly IValidator<TestSendNotificationDto> _testSendValidator;

    public NotificationController(INotificationService service, IHubContext<NotificationsHub> hub,
        IEmailSender emailSender, ISmsSender smsSender, IPushSender pushSender,
        IValidator<TestSendNotificationDto> testSendValidator)
    {
        _service = service;
        _hub = hub;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
        _testSendValidator = testSendValidator;
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

    /// <summary>Raw connectivity test for an admin verifying SMTP/SMS/Push config — sends
    /// directly to an explicit address rather than through IMultiChannelNotificationDispatcher,
    /// since there's no existing in-app Notification/user to tie this to.</summary>
    [Authorize(Policy = "settings.edit")]
    [HttpPost("test-send")]
    public async Task<IActionResult> TestSend([FromBody] TestSendNotificationDto dto)
    {
        await _testSendValidator.ValidateAndThrowAsync(dto);

        var result = dto.Channel switch
        {
            "Email" => await _emailSender.SendAsync(OrgId, dto.To, dto.Subject ?? "NovaERP test notification", dto.Message),
            "Sms" => await _smsSender.SendAsync(OrgId, dto.To, dto.Message),
            "Push" => await _pushSender.SendAsync(OrgId, dto.To, dto.Subject ?? "NovaERP", dto.Message),
            _ => new SendResultDto(false, "Failed", $"Unknown channel '{dto.Channel}'.")
        };

        return Ok(result);
    }
}
