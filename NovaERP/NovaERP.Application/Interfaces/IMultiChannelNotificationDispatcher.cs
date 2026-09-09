using NovaERP.Application.Common;

namespace NovaERP.Application.Interfaces;

/// <summary>Fans an event out across the requested channels for one user. Always writes the
/// in-app Notification via INotificationService when NotificationChannels.InApp is set, and
/// attempts Email/Sms/Push for whichever other flags are set, logging one NotificationDeliveryLog
/// row per channel attempted. This method must never throw — a failed SMS/push/email must never
/// break the business operation that triggered it.</summary>
public interface IMultiChannelNotificationDispatcher
{
    Task DispatchAsync(int organizationId, int userId, string title, string message, NotificationChannels channels);
}
