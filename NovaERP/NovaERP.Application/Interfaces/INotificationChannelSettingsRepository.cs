using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface INotificationChannelSettingsRepository
{
    Task<NotificationChannelSettings?> GetByOrgAsync(int orgId);
    void Add(NotificationChannelSettings settings);
    void Update(NotificationChannelSettings settings);
    Task SaveChangesAsync();
}
