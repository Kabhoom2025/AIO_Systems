using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>Fans a notification out across channels for one user. Design notes:
/// - The in-app Notification is always created (unconditionally, regardless of which flags are
///   requested) because NotificationDeliveryLog rows need a NotificationId to reference — this
///   keeps every dispatch attempt (including Email/Sms/Push-only requests) auditable against one
///   parent record.
/// - User has no PhoneNumber/DeviceToken column in this phase (a deliberate scope choice — see
///   IMultiChannelNotificationDispatcher design notes in the Phase 2 report); Sms/Push dispatch
///   by user-id therefore always resolves to "NotConfigured" rather than a schema change. The
///   lower-level ISmsSender/IPushSender/IEmailSender are still directly callable with an explicit
///   address (see NotificationController.TestSend) so the feature remains demonstrable.
/// - This method never throws: every failure mode (missing config, transport error, even a
///   failure to create the in-app Notification itself) is caught and swallowed/logged, since a
///   failed notification channel must never break the business operation that triggered it.</summary>
public class MultiChannelNotificationDispatcher : IMultiChannelNotificationDispatcher
{
    private readonly NovaErpDbContext _ctx;
    private readonly INotificationService _notificationService;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IPushSender _pushSender;

    public MultiChannelNotificationDispatcher(
        NovaErpDbContext ctx,
        INotificationService notificationService,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IPushSender pushSender)
    {
        _ctx = ctx;
        _notificationService = notificationService;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _pushSender = pushSender;
    }

    public async Task DispatchAsync(int organizationId, int userId, string title, string message, NotificationChannels channels)
    {
        try
        {
            NotificationDto notification;
            try
            {
                notification = await _notificationService.CreateAsync(organizationId, new CreateNotificationDto
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = "Info"
                });
            }
            catch
            {
                return; // even the in-app write failed; nothing to log against, bail out quietly.
            }

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId && u.OrganizationId == organizationId);

            if (channels.HasFlag(NotificationChannels.Email))
            {
                var result = string.IsNullOrWhiteSpace(user?.Email)
                    ? new SendResultDto(false, "NotConfigured", "User has no email address on file.")
                    : await _emailSender.SendAsync(organizationId, user.Email, title, message);
                await LogAsync(notification.Id, "Email", result);
            }

            if (channels.HasFlag(NotificationChannels.Sms))
            {
                // No PhoneNumber column on User in this phase — see class remarks.
                var result = new SendResultDto(false, "NotConfigured", "User has no phone number on file.");
                await LogAsync(notification.Id, "Sms", result);
            }

            if (channels.HasFlag(NotificationChannels.Push))
            {
                // No device-token column on User in this phase — see class remarks.
                var result = new SendResultDto(false, "NotConfigured", "User has no registered device token.");
                await LogAsync(notification.Id, "Push", result);
            }
        }
        catch
        {
            // Absolute last resort — this method must never throw.
        }
    }

    private async Task LogAsync(int notificationId, string channel, SendResultDto result)
    {
        _ctx.NotificationDeliveryLogs.Add(new NotificationDeliveryLog
        {
            NotificationId = notificationId,
            Channel = channel,
            Status = result.Status,
            Error = result.Error,
            SentDate = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();
    }
}
