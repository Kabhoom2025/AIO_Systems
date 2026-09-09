using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetListAsync(int orgId, int take = 50);
    Task<int> GetUnreadCountAsync(int orgId);
    Task<List<NotificationDto>> GenerateAndGetNewAsync(int orgId);
    Task MarkReadAsync(int id);
    Task MarkAllReadAsync(int orgId);
}
