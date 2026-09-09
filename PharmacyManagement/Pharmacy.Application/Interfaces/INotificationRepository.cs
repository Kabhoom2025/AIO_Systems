using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetRecentByOrgAsync(int orgId, int take);
    Task<int> GetUnreadCountAsync(int orgId);
    Task<bool> ExistsForTodayAsync(int orgId, string type, int relatedEntityId);
    Task<Notification?> GetByIdAsync(int id);
    Task<List<Notification>> GetUnreadByOrgAsync(int orgId);
    void Add(Notification notification);
    Task SaveChangesAsync();
}
