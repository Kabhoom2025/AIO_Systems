namespace NovaERP.Application.Interfaces;

using NovaERP.Application.DTOs;

/// <summary>Generic HTTP SMS gateway adapter — point SmsApiUrl/SmsApiKey at a real provider
/// (e.g. Twilio, Msg91) to activate. Returns "NotConfigured" (never throws) when blank.</summary>
public interface ISmsSender
{
    Task<SendResultDto> SendAsync(int organizationId, string toPhoneNumber, string message);
}
