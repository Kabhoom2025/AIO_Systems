using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly NovaErpDbContext _ctx;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateNotificationDto> _createValidator;

    public NotificationService(NovaErpDbContext ctx, IMapper mapper, IValidator<CreateNotificationDto> createValidator)
    {
        _ctx = ctx;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<List<NotificationDto>> GetForUserAsync(int orgId, int userId, int take = 50)
    {
        var notifications = await _ctx.Notifications
            .Where(n => n.OrganizationId == orgId && (n.UserId == null || n.UserId == userId))
            .OrderByDescending(n => n.CreatedDate)
            .Take(take)
            .ToListAsync();
        return notifications.Select(_mapper.Map<NotificationDto>).ToList();
    }

    public Task<int> GetUnreadCountAsync(int orgId, int userId) =>
        _ctx.Notifications.CountAsync(n =>
            n.OrganizationId == orgId && (n.UserId == null || n.UserId == userId) && !n.IsRead);

    public async Task<NotificationDto> CreateAsync(int orgId, CreateNotificationDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var notification = new Notification
        {
            OrganizationId = orgId,
            UserId         = dto.UserId,
            Title          = dto.Title,
            Message        = dto.Message,
            Type           = dto.Type,
            Link           = dto.Link
        };
        _ctx.Notifications.Add(notification);
        await _ctx.SaveChangesAsync();
        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task MarkReadAsync(int id, int userId)
    {
        var notification = await _ctx.Notifications.FirstOrDefaultAsync(n => n.Id == id)
            ?? throw new KeyNotFoundException($"Notification {id} not found");
        notification.IsRead = true;
        notification.UpdatedDate = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int orgId, int userId)
    {
        await _ctx.Notifications
            .Where(n => n.OrganizationId == orgId && (n.UserId == null || n.UserId == userId) && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.UpdatedDate, DateTime.UtcNow));
    }
}
