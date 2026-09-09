using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>Generic HTTP SMS gateway adapter — point NotificationChannelSettings.SmsApiUrl /
/// SmsApiKey at a real provider (e.g. Twilio, Msg91) to activate. Posts a small JSON payload
/// {to, message, senderId} and treats the ApiKey as a bearer credential; adapt the payload shape
/// in one place here if your provider expects a different body. Returns "NotConfigured" when
/// blank and "Failed" (never throws) on any non-2xx response or transport error.</summary>
public class HttpSmsSender : ISmsSender
{
    private readonly NovaErpDbContext _ctx;
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpSmsSender(NovaErpDbContext ctx, IHttpClientFactory httpClientFactory)
    {
        _ctx = ctx;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SendResultDto> SendAsync(int organizationId, string toPhoneNumber, string message)
    {
        var settings = await _ctx.NotificationChannelSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId);

        if (settings == null || string.IsNullOrWhiteSpace(settings.SmsApiUrl) || string.IsNullOrWhiteSpace(settings.SmsApiKey))
            return new SendResultDto(false, "NotConfigured", "SMS gateway is not configured for this organization.");

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(HttpSmsSender));
            using var request = new HttpRequestMessage(HttpMethod.Post, settings.SmsApiUrl)
            {
                Content = JsonContent.Create(new
                {
                    to = toPhoneNumber,
                    message,
                    senderId = settings.SmsSenderId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SmsApiKey);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return new SendResultDto(false, "Failed", $"SMS gateway returned {(int)response.StatusCode}: {body}");
            }

            return new SendResultDto(true, "Sent", null);
        }
        catch (Exception ex)
        {
            return new SendResultDto(false, "Failed", ex.Message);
        }
    }
}
