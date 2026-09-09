using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

/// <summary>Generic HTTP push gateway adapter (FCM-legacy-HTTP-API-shaped body) — point
/// PushApiUrl/PushServerKey at a real provider to activate. Returns "NotConfigured" (never
/// throws) when blank.</summary>
public interface IPushSender
{
    Task<SendResultDto> SendAsync(int organizationId, string deviceToken, string title, string message);
}
