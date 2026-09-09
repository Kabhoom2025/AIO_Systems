using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatbot.Infrastructure.Ai;

/// <summary>Talks to OpenAI's Chat Completions API (https://api.openai.com/v1/chat/completions).</summary>
public class OpenAiChatService(HttpClient httpClient, IOptions<AiOptions> options, ILogger<OpenAiChatService> logger)
    : OpenAiCompatibleChatServiceBase(httpClient, options, logger)
{
    public override string ProviderName => "OpenAI";

    protected override string BuildRequestUri() =>
        $"{(string.IsNullOrWhiteSpace(Options.BaseUrl) ? "https://api.openai.com/v1" : Options.BaseUrl).TrimEnd('/')}/chat/completions";

    protected override void ApplyAuthHeaders(HttpRequestMessage request) =>
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Options.ApiKey);
}
