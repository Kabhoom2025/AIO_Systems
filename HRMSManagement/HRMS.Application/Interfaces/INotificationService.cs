using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetForUserAsync(int orgId, int userId, int take = 50);
    Task<int> GetUnreadCountAsync(int orgId, int userId);
    Task<NotificationDto> CreateAsync(int orgId, CreateNotificationDto dto);
    Task MarkReadAsync(int id, int userId);
    Task MarkAllReadAsync(int orgId, int userId);
}
