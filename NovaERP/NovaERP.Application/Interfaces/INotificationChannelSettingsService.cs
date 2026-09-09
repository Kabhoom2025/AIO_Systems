using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface INotificationChannelSettingsService
{
    Task<NotificationChannelSettingsDto> GetAsync(int orgId);
    Task<NotificationChannelSettingsDto> UpdateAsync(int orgId, UpdateNotificationChannelSettingsDto dto);
}
