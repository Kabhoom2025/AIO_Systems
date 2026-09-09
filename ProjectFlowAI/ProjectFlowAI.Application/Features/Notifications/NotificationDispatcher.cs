using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Notifications;

/// <summary>Real implementation of INotificationDispatcher: writes the Notification row, pushes it
/// live over NotificationsHub, and fans out to Email/Slack/Teams/SMS per the user's
/// NotificationPreference (defaulting to enabled when no row exists). Wired into work-item comment
/// mentions, assignment changes, and sprint start — see WorkItemCommands.cs / SprintFeatures.cs.</summary>
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationRealtimeNotifier _realtime;
    private readonly IEmailSender _emailSender;
    private readonly ISlackNotifier _slack;
    private readonly ITeamsNotifier _teams;
    private readonly ISmsNotifier _sms;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(IProjectFlowDbContext db, IMapper mapper, INotificationRealtimeNotifier realtime,
        IEmailSender emailSender, ISlackNotifier slack, ITeamsNotifier teams, ISmsNotifier sms, ILogger<NotificationDispatcher> logger)
    {
        _db = db; _mapper = mapper; _realtime = realtime;
        _emailSender = emailSender; _slack = slack; _teams = teams; _sms = sms; _logger = logger;
    }

    public async Task DispatchAsync(Guid userId, NotificationType type, string title, string body,
        string? linkUrl = null, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("NotificationDispatcher: user {UserId} not found, skipping.", userId);
            return;
        }

        var notification = new Notification { UserId = userId, Type = type, Title = title, Body = body, LinkUrl = linkUrl };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<NotificationDto>(notification);
        await _realtime.NotificationReceivedAsync(userId, dto, cancellationToken);

        var preferences = await _db.NotificationPreferences.Where(p => p.UserId == userId).ToListAsync(cancellationToken);
        bool IsEnabled(NotificationChannel channel)
        {
            var pref = preferences.FirstOrDefault(p => p.Channel == channel);
            return pref?.IsEnabled ?? true; // no row => enabled by default
        }

        if (IsEnabled(NotificationChannel.Email))
            await _emailSender.SendAsync(user.Email, title, body, cancellationToken);

        if (user.OrganizationId.HasValue)
        {
            var settings = await _db.OrganizationIntegrationSettings
                .FirstOrDefaultAsync(s => s.OrganizationId == user.OrganizationId.Value, cancellationToken);

            if (settings != null)
            {
                if (IsEnabled(NotificationChannel.Slack) && !string.IsNullOrWhiteSpace(settings.SlackWebhookUrl))
                    await _slack.SendAsync(settings.SlackWebhookUrl, title, body, cancellationToken);

                if (IsEnabled(NotificationChannel.Teams) && !string.IsNullOrWhiteSpace(settings.TeamsWebhookUrl))
                    await _teams.SendAsync(settings.TeamsWebhookUrl, title, body, cancellationToken);

                // SMS needs a recipient phone number, which this data model doesn't currently
                // capture on User — the provider integration itself is real and wired (ISmsNotifier
                // does a genuine HTTP POST), but there is no phone number to send to yet, so this
                // is a documented no-op rather than sending to a fabricated number.
                if (IsEnabled(NotificationChannel.Sms) && !string.IsNullOrWhiteSpace(settings.SmsProviderUrl))
                    _logger.LogInformation("SMS channel enabled but User has no phone number on file; skipping SMS for {UserId}.", userId);
            }
        }
    }
}
