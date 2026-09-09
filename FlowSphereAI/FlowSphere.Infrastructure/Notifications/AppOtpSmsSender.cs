using System.Net.Http.Headers;
using System.Text;
using FlowSphere.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowSphere.Infrastructure.Notifications;

/// <summary>Mirrors FlowSphere.Execution's TwilioConnector/TwilioSmsNodeExecutor pair (same
/// built-in "Twilio" Connector row, same ConnectorCredential keys: AccountSid/AuthToken/
/// FromNumber) but lives in Infrastructure so app-level OTP delivery doesn't need a dependency on
/// the Execution project.</summary>
public class AppOtpSmsSender : IAppOtpSmsSender
{
    private readonly IApplicationDbContext _db;
    private readonly IConnectorCredentialStore _credentialStore;
    private readonly IHttpClientFactory _httpClientFactory;

    public AppOtpSmsSender(IApplicationDbContext db, IConnectorCredentialStore credentialStore, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _credentialStore = credentialStore;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SmsSendResult> SendAsync(int organizationId, string toPhoneNumber, string body, CancellationToken cancellationToken)
    {
        var twilioConnectorId = await _db.Connectors
            .Where(c => c.Type == "Twilio" && c.OrganizationId == null)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (twilioConnectorId == 0)
        {
            return new SmsSendResult(false, "No built-in Twilio connector is configured.");
        }

        var accountSid = await _credentialStore.GetAsync(twilioConnectorId, "AccountSid", organizationId, cancellationToken);
        var authToken = await _credentialStore.GetAsync(twilioConnectorId, "AuthToken", organizationId, cancellationToken);
        var fromNumber = await _credentialStore.GetAsync(twilioConnectorId, "FromNumber", organizationId, cancellationToken);

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(fromNumber))
        {
            return new SmsSendResult(false, "This organization has no Twilio credentials configured - OTP SMS was not sent.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(
            HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = toPhoneNumber,
                ["From"] = fromNumber,
                ["Body"] = body,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{accountSid}:{authToken}")));

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                return new SmsSendResult(false, $"Twilio error ({(int)response.StatusCode}): {responseBody}");
            }

            return new SmsSendResult(true, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new SmsSendResult(false, $"Twilio request error: {ex.Message}");
        }
    }
}
