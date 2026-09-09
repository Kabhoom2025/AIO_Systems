using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

/// <summary>Generic HTTP push gateway adapter — point NotificationChannelSettings.PushApiUrl /
/// PushServerKey at a real provider to activate. Body shape mirrors the FCM legacy HTTP API
/// (`to`, `notification: { title, body }`) since that's a reasonable, widely-compatible default;
/// adapt in one place here if your provider expects a different shape. Returns "NotConfigured"
/// when blank and "Failed" (never throws) on any non-2xx response or transport error.</summary>
public class HttpPushSender : IPushSender
{
    private readonly NovaErpDbContext _ctx;
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpPushSender(NovaErpDbContext ctx, IHttpClientFactory httpClientFactory)
    {
        _ctx = ctx;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SendResultDto> SendAsync(int organizationId, string deviceToken, string title, string message)
    {
        var settings = await _ctx.NotificationChannelSettings
            .FirstOrDefaultAsync(s => s.OrganizationId == organizationId);

        if (settings == null || string.IsNullOrWhiteSpace(settings.PushApiUrl) || string.IsNullOrWhiteSpace(settings.PushServerKey))
            return new SendResultDto(false, "NotConfigured", "Push gateway is not configured for this organization.");

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(HttpPushSender));
            using var request = new HttpRequestMessage(HttpMethod.Post, settings.PushApiUrl)
            {
                Content = JsonContent.Create(new
                {
                    to = deviceToken,
                    notification = new { title, body = message }
                })
            };
            request.Headers.TryAddWithoutValidation("Authorization", $"key={settings.PushServerKey}");

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return new SendResultDto(false, "Failed", $"Push gateway returned {(int)response.StatusCode}: {body}");
            }

            return new SendResultDto(true, "Sent", null);
        }
        catch (Exception ex)
        {
            return new SendResultDto(false, "Failed", ex.Message);
        }
    }
}
