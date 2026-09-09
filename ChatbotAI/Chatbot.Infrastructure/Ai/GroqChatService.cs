using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatbot.Infrastructure.Ai;

/// <summary>
/// Talks to Groq's fast-inference API (https://api.groq.com/openai/v1), which exposes an
/// OpenAI-compatible chat-completions endpoint. Not to be confused with xAI's "Grok" model
/// (<see cref="GrokChatService"/>) — different company, different API keys ("gsk_..." vs
/// "xai-..."), same wire format.
/// </summary>
public class GroqChatService(HttpClient httpClient, IOptions<AiOptions> options, ILogger<GroqChatService> logger)
    : OpenAiCompatibleChatServiceBase(httpClient, options, logger)
{
    public override string ProviderName => "Groq";

    protected override string BuildRequestUri() =>
        $"{(string.IsNullOrWhiteSpace(Options.BaseUrl) ? "https://api.groq.com/openai/v1" : Options.BaseUrl).TrimEnd('/')}/chat/completions";

    protected override void ApplyAuthHeaders(HttpRequestMessage request) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Options.ApiKey);
}
