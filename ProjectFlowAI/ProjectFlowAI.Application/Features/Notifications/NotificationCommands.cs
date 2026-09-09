using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Notifications;

public record MarkNotificationReadCommand(Guid Id, Guid UserId) : IRequest<Unit>;

public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public MarkNotificationReadCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Notification", request.Id);
        if (notification.UserId != request.UserId)
            throw new UnauthorizedDomainException("This notification does not belong to you.");

        notification.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record MarkAllNotificationsReadCommand(Guid UserId) : IRequest<Unit>;

public class MarkAllNotificationsReadCommandValidator : AbstractValidator<MarkAllNotificationsReadCommand>
{
    public MarkAllNotificationsReadCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}

public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public MarkAllNotificationsReadCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == request.UserId && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record UpdateNotificationPreferenceCommand(Guid UserId, NotificationChannel Channel, bool IsEnabled) : IRequest<Unit>;

public class UpdateNotificationPreferenceCommandValidator : AbstractValidator<UpdateNotificationPreferenceCommand>
{
    public UpdateNotificationPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Channel).IsInEnum();
    }
}

public class UpdateNotificationPreferenceCommandHandler : IRequestHandler<UpdateNotificationPreferenceCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public UpdateNotificationPreferenceCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var pref = await _db.NotificationPreferences.FirstOrDefaultAsync(
            p => p.UserId == request.UserId && p.Channel == request.Channel, cancellationToken);

        if (pref == null)
        {
            pref = new NotificationPreference { UserId = request.UserId, Channel = request.Channel };
            _db.NotificationPreferences.Add(pref);
        }
        pref.IsEnabled = request.IsEnabled;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record UpdateOrganizationIntegrationSettingsCommand(Guid OrganizationId, string? SlackWebhookUrl,
    string? TeamsWebhookUrl, string? SmsProviderUrl, string? SmsProviderApiKey) : IRequest<Unit>;

public class UpdateOrganizationIntegrationSettingsCommandValidator : AbstractValidator<UpdateOrganizationIntegrationSettingsCommand>
{
    public UpdateOrganizationIntegrationSettingsCommandValidator() => RuleFor(x => x.OrganizationId).NotEmpty();
}

public class UpdateOrganizationIntegrationSettingsCommandHandler : IRequestHandler<UpdateOrganizationIntegrationSettingsCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public UpdateOrganizationIntegrationSettingsCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateOrganizationIntegrationSettingsCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);

        var settings = await _db.OrganizationIntegrationSettings.FirstOrDefaultAsync(s => s.OrganizationId == request.OrganizationId, cancellationToken);
        if (settings == null)
        {
            settings = new OrganizationIntegrationSettings { OrganizationId = request.OrganizationId };
            _db.OrganizationIntegrationSettings.Add(settings);
        }

        settings.SlackWebhookUrl = request.SlackWebhookUrl;
        settings.TeamsWebhookUrl = request.TeamsWebhookUrl;
        settings.SmsProviderUrl = request.SmsProviderUrl;
        settings.SmsProviderApiKey = request.SmsProviderApiKey;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
