using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class NotificationService : INotificationService
{
    private const int ExpiryThresholdDays = 30;

    private readonly INotificationRepository _repo;
    private readonly IMedicineRepository _medicineRepo;

    public NotificationService(INotificationRepository repo, IMedicineRepository medicineRepo)
    {
        _repo = repo;
        _medicineRepo = medicineRepo;
    }

    public async Task<List<NotificationDto>> GetListAsync(int orgId, int take = 50)
    {
        await GenerateAlertsAsync(orgId);
        var notifications = await _repo.GetRecentByOrgAsync(orgId, take);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(int orgId)
    {
        await GenerateAlertsAsync(orgId);
        return await _repo.GetUnreadCountAsync(orgId);
    }

    public async Task<List<NotificationDto>> GenerateAndGetNewAsync(int orgId)
    {
        var newOnes = await GenerateAlertsAsync(orgId);
        return newOnes.Select(MapToDto).ToList();
    }

    public async Task MarkReadAsync(int id)
    {
        var notification = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Notification {id} not found");
        notification.IsRead = true;
        notification.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int orgId)
    {
        var unread = await _repo.GetUnreadByOrgAsync(orgId);
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.UpdatedDate = DateTime.UtcNow;
        }
        await _repo.SaveChangesAsync();
    }

    private async Task<List<Notification>> GenerateAlertsAsync(int orgId)
    {
        var medicines = await _medicineRepo.GetAllByOrgAsync(orgId);
        var newNotifications = new List<Notification>();

        foreach (var m in medicines.Where(m => m.IsActive))
        {
            var totalStock = m.Batches.Sum(b => b.CurrentQuantity);
            if (totalStock > m.ReorderLevel) continue;
            if (await _repo.ExistsForTodayAsync(orgId, "LowStock", m.Id)) continue;

            var notification = new Notification
            {
                OrganizationId    = orgId,
                Type              = "LowStock",
                Title             = "Low stock alert",
                Message           = $"{m.Name} is low on stock ({totalStock} {m.Unit} remaining, reorder level {m.ReorderLevel}).",
                RelatedEntityType = "Medicine",
                RelatedEntityId   = m.Id
            };
            _repo.Add(notification);
            newNotifications.Add(notification);
        }

        var expiringBatches = await _medicineRepo.GetExpiryAlertsAsync(orgId, ExpiryThresholdDays);
        foreach (var b in expiringBatches)
        {
            if (await _repo.ExistsForTodayAsync(orgId, "ExpiringBatch", b.Id)) continue;

            var days = (b.ExpiryDate.Date - DateTime.UtcNow.Date).Days;
            var notification = new Notification
            {
                OrganizationId    = orgId,
                Type              = "ExpiringBatch",
                Title             = "Expiring batch alert",
                Message           = days < 0
                    ? $"{b.Medicine.Name} batch {b.BatchNumber} has expired."
                    : $"{b.Medicine.Name} batch {b.BatchNumber} expires in {days} day(s).",
                RelatedEntityType = "MedicineBatch",
                RelatedEntityId   = b.Id
            };
            _repo.Add(notification);
            newNotifications.Add(notification);
        }

        if (newNotifications.Count > 0) await _repo.SaveChangesAsync();
        return newNotifications;
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id                = n.Id,
        Type              = n.Type,
        Title             = n.Title,
        Message           = n.Message,
        RelatedEntityType = n.RelatedEntityType,
        RelatedEntityId   = n.RelatedEntityId,
        IsRead            = n.IsRead,
        CreatedDate       = n.CreatedDate
    };
}
