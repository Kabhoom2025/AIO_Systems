namespace NovaERP.Application.Common;

/// <summary>Bitwise-combinable set of delivery channels a caller can request when fanning out a
/// notification via IMultiChannelNotificationDispatcher.</summary>
[Flags]
public enum NotificationChannels
{
    None  = 0,
    InApp = 1,
    Email = 2,
    Sms   = 4,
    Push  = 8
}
