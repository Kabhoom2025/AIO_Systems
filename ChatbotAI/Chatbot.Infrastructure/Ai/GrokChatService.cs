using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatbot.Infrastructure.Ai;

/// <summary>Talks to xAI's Grok API, which exposes an OpenAI-compatible chat-completions endpoint at https://api.x.ai/v1.</summary>
public class GrokChatService(HttpClient httpClient, IOptions<AiOptions> options, ILogger<GrokChatService> logger)
    : OpenAiCompatibleChatServiceBase(httpClient, options, logger)
{
    public override string ProviderName => "Grok";

    protected override string BuildRequestUri() =>
        $"{(string.IsNullOrWhiteSpace(Options.BaseUrl) ? "https://api.x.ai/v1" : Options.BaseUrl).TrimEnd('/')}/chat/completions";

    protected override void ApplyAuthHeaders(HttpRequestMessage request) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Options.ApiKey);
}
