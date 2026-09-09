using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly PharmacyDbContext _ctx;

    public NotificationRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Notification>> GetRecentByOrgAsync(int orgId, int take) =>
        _ctx.Notifications
            .Where(n => n.OrganizationId == orgId)
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedDate)
            .Take(take)
            .ToListAsync();

    public Task<int> GetUnreadCountAsync(int orgId) =>
        _ctx.Notifications.CountAsync(n => n.OrganizationId == orgId && !n.IsRead);

    public Task<bool> ExistsForTodayAsync(int orgId, string type, int relatedEntityId)
    {
        var today = DateTime.UtcNow.Date;
        return _ctx.Notifications.AnyAsync(n =>
            n.OrganizationId == orgId &&
            n.Type == type &&
            n.RelatedEntityId == relatedEntityId &&
            n.CreatedDate >= today);
    }

    public Task<Notification?> GetByIdAsync(int id) =>
        _ctx.Notifications.FirstOrDefaultAsync(n => n.Id == id);

    public Task<List<Notification>> GetUnreadByOrgAsync(int orgId) =>
        _ctx.Notifications.Where(n => n.OrganizationId == orgId && !n.IsRead).ToListAsync();

    public void Add(Notification notification) => _ctx.Notifications.Add(notification);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
