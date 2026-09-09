using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly HrmsDbContext _ctx;

    public NotificationService(HrmsDbContext ctx) => _ctx = ctx;

    public async Task<List<NotificationDto>> GetForUserAsync(int orgId, int userId, int take = 50)
    {
        return await _ctx.Notifications
            .Where(n => n.OrganizationId == orgId && (n.UserId == null || n.UserId == userId))
            .OrderByDescending(n => n.CreatedDate)
            .Take(take)
            .Select(n => MapToDto(n))
            .ToListAsync();
    }

    public Task<int> GetUnreadCountAsync(int orgId, int userId) =>
        _ctx.Notifications.CountAsync(n =>
            n.OrganizationId == orgId && (n.UserId == null || n.UserId == userId) && !n.IsRead);

    public async Task<NotificationDto> CreateAsync(int orgId, CreateNotificationDto dto)
    {
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
        return MapToDto(notification);
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

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id          = n.Id,
        UserId      = n.UserId,
        Title       = n.Title,
        Message     = n.Message,
        Type        = n.Type,
        Link        = n.Link,
        IsRead      = n.IsRead,
        CreatedDate = n.CreatedDate
    };
}
