using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Application.Features.Notifications;

public record ListNotificationsQuery(Guid UserId, int Page, int PageSize, bool UnreadOnly) : IRequest<PagedResult<NotificationDto>>;

public class ListNotificationsQueryHandler : IRequestHandler<ListNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListNotificationsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<NotificationDto>> Handle(ListNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Notifications.Where(n => n.UserId == request.UserId);
        if (request.UnreadOnly) query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(cancellationToken);
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var entities = await query.OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>(entities.Select(e => _mapper.Map<NotificationDto>(e)).ToList(), total, page, pageSize);
    }
}

public record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<UnreadCountDto>;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, UnreadCountDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetUnreadNotificationCountQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<UnreadCountDto> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _db.Notifications.CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);
        return new UnreadCountDto(count);
    }
}

public record GetNotificationPreferencesQuery(Guid UserId) : IRequest<IReadOnlyList<NotificationPreferenceDto>>;

public class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, IReadOnlyList<NotificationPreferenceDto>>
{
    private static readonly NotificationChannel[] AllChannels =
        { NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Slack, NotificationChannel.Teams, NotificationChannel.Sms };

    private readonly IProjectFlowDbContext _db;

    public GetNotificationPreferencesQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationPreferenceDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var existing = await _db.NotificationPreferences.Where(p => p.UserId == request.UserId).ToDictionaryAsync(p => p.Channel, cancellationToken);

        // Missing rows default to enabled=true rather than erroring — a brand-new user has never
        // explicitly opted out of anything.
        return AllChannels.Select(channel => new NotificationPreferenceDto(channel, existing.TryGetValue(channel, out var pref) ? pref.IsEnabled : true)).ToList();
    }
}

public record GetOrganizationIntegrationSettingsQuery(Guid OrganizationId) : IRequest<OrganizationIntegrationSettingsDto>;

public class GetOrganizationIntegrationSettingsQueryHandler : IRequestHandler<GetOrganizationIntegrationSettingsQuery, OrganizationIntegrationSettingsDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetOrganizationIntegrationSettingsQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<OrganizationIntegrationSettingsDto> Handle(GetOrganizationIntegrationSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _db.OrganizationIntegrationSettings.FirstOrDefaultAsync(s => s.OrganizationId == request.OrganizationId, cancellationToken);
        // Never returns SmsProviderApiKey — it's write-only, per the API contract.
        return new OrganizationIntegrationSettingsDto(settings?.SlackWebhookUrl, settings?.TeamsWebhookUrl, settings?.SmsProviderUrl);
    }
}
