using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Services;

/// <summary>Real HTTP POST to a configured Slack incoming-webhook URL. Degrades to logging (never
/// throws) when webhookUrl is empty — an org that hasn't configured Slack yet just doesn't get a
/// Slack ping, exactly the same degraded-path convention as SmtpEmailSender.</summary>
public class SlackNotifier : ISlackNotifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SlackNotifier> _logger;

    public SlackNotifier(IHttpClientFactory httpClientFactory, ILogger<SlackNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string webhookUrl, string title, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogInformation("[SlackNotifier:LogOnly] Title={Title} Body={Body}", title, body);
            return;
        }

        var client = _httpClientFactory.CreateClient();
        // Slack's incoming-webhook contract: a JSON body with a top-level "text" field.
        var response = await client.PostAsJsonAsync(webhookUrl, new { text = $"*{title}*\n{body}" }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("Slack webhook POST returned {StatusCode}", response.StatusCode);
    }
}

/// <summary>Real HTTP POST to a configured Microsoft Teams incoming-webhook URL (MessageCard
/// format). Degrades to logging when webhookUrl is empty.</summary>
public class TeamsNotifier : ITeamsNotifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TeamsNotifier> _logger;

    public TeamsNotifier(IHttpClientFactory httpClientFactory, ILogger<TeamsNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string webhookUrl, string title, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogInformation("[TeamsNotifier:LogOnly] Title={Title} Body={Body}", title, body);
            return;
        }

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(webhookUrl, new
        {
            @type = "MessageCard",
            @context = "http://schema.org/extensions",
            themeColor = "6366F1",
            title,
            text = body
        }, cancellationToken);
        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("Teams webhook POST returned {StatusCode}", response.StatusCode);
    }
}

/// <summary>Generic HTTP SMS provider integration — POSTs {to, body} plus an Authorization: Bearer
/// {apiKey} header to a configurable provider endpoint (this shape is deliberately
/// provider-agnostic; swap in a Twilio/Vonage/etc.-specific payload once a real provider is
/// selected). Degrades to logging when providerUrl is empty.</summary>
public class SmsNotifier : ISmsNotifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SmsNotifier> _logger;

    public SmsNotifier(IHttpClientFactory httpClientFactory, ILogger<SmsNotifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string providerUrl, string apiKey, string toPhoneNumber, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(providerUrl))
        {
            _logger.LogInformation("[SmsNotifier:LogOnly] To={To} Body={Body}", toPhoneNumber, body);
            return;
        }

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, providerUrl)
        {
            Content = JsonContent.Create(new { to = toPhoneNumber, body })
        };
        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("SMS provider POST returned {StatusCode}", response.StatusCode);
    }
}
