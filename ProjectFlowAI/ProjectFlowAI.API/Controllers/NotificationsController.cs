using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Notifications;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.API.Controllers;

[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedDomainException("Authenticated request is missing a user id claim.");

    public record UpdatePreferenceRequest(NotificationChannel Channel, bool IsEnabled);
    public record UpdateIntegrationSettingsRequest(string? SlackWebhookUrl, string? TeamsWebhookUrl, string? SmsProviderUrl, string? SmsProviderApiKey);

    [HttpGet("api/notifications")]
    public async Task<ActionResult<PagedResult<NotificationDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false)
    {
        var result = await _mediator.Send(new ListNotificationsQuery(CurrentUserId, page, pageSize, unreadOnly));
        return Ok(result);
    }

    [HttpPost("api/notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id, CurrentUserId));
        return NoContent();
    }

    [HttpPost("api/notifications/read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(CurrentUserId));
        return NoContent();
    }

    [HttpGet("api/notifications/unread-count")]
    public async Task<ActionResult<UnreadCountDto>> UnreadCount()
    {
        var result = await _mediator.Send(new GetUnreadNotificationCountQuery(CurrentUserId));
        return Ok(result);
    }

    [HttpGet("api/notification-preferences")]
    public async Task<ActionResult<IReadOnlyList<NotificationPreferenceDto>>> GetPreferences()
    {
        var result = await _mediator.Send(new GetNotificationPreferencesQuery(CurrentUserId));
        return Ok(result);
    }

    [HttpPut("api/notification-preferences")]
    public async Task<IActionResult> UpdatePreference(UpdatePreferenceRequest request)
    {
        await _mediator.Send(new UpdateNotificationPreferenceCommand(CurrentUserId, request.Channel, request.IsEnabled));
        return NoContent();
    }

    [HttpGet("api/organizations/{id:guid}/integration-settings")]
    [Authorize(Policy = PermissionCatalog.NotificationsManage)]
    public async Task<ActionResult<OrganizationIntegrationSettingsDto>> GetIntegrationSettings(Guid id)
    {
        var result = await _mediator.Send(new GetOrganizationIntegrationSettingsQuery(id));
        return Ok(result);
    }

    [HttpPut("api/organizations/{id:guid}/integration-settings")]
    [Authorize(Policy = PermissionCatalog.NotificationsManage)]
    public async Task<IActionResult> UpdateIntegrationSettings(Guid id, UpdateIntegrationSettingsRequest request)
    {
        await _mediator.Send(new UpdateOrganizationIntegrationSettingsCommand(
            id, request.SlackWebhookUrl, request.TeamsWebhookUrl, request.SmsProviderUrl, request.SmsProviderApiKey));
        return NoContent();
    }
}
