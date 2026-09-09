using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlowSphere.Execution.Connectors;

/// <summary>Config shape (merged by AiPromptNodeExecutor): { "apiKey": "...", "model":
/// "gpt-4o-mini", "prompt": "...", "baseUrl": "<optional, defaults to OpenAI's endpoint>" }.
/// Calls a Chat Completions-shaped API directly over HttpClient - no SDK dependency needed for
/// a single-endpoint integration. The "baseUrl" override means this same connector works with
/// any OpenAI-compatible provider (e.g. xAI/Grok's API is intentionally OpenAI-request-shape
/// compatible at https://api.x.ai/v1/chat/completions) without a separate connector type.</summary>
public class OpenAiConnector : IConnector
{
    public string Type => "OpenAi";

    private const string DefaultChatCompletionsUrl = "https://api.openai.com/v1/chat/completions";

    private readonly IHttpClientFactory _httpClientFactory;

    public OpenAiConnector(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ConnectorResult> InvokeAsync(ConnectorInvocation invocation, CancellationToken cancellationToken)
    {
        using var config = JsonDocument.Parse(invocation.ConfigJson);
        var root = config.RootElement;

        var apiKey = root.TryGetProperty("apiKey", out var keyEl) ? keyEl.GetString() : null;
        var model = root.TryGetProperty("model", out var modelEl) ? modelEl.GetString() : null;
        var prompt = root.TryGetProperty("prompt", out var promptEl) ? promptEl.GetString() : null;
        var baseUrl = root.TryGetProperty("baseUrl", out var baseUrlEl) ? baseUrlEl.GetString() : null;
        var endpoint = string.IsNullOrWhiteSpace(baseUrl) ? DefaultChatCompletionsUrl : baseUrl;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ConnectorResult.Fail("No API key configured for this organization.");
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            return ConnectorResult.Fail("AI Prompt node config is missing a 'prompt'.");
        }

        var client = _httpClientFactory.CreateClient("FlowSphereConnectors");
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new ChatCompletionRequest(
                model ?? "gpt-4o-mini",
                new[] { new ChatMessage("user", prompt) })),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ConnectorResult.Fail($"AI provider error ({(int)response.StatusCode}): {body}");
            }

            var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(body);
            var content = completion?.Choices.FirstOrDefault()?.Message.Content ?? "";

            return ConnectorResult.Ok(JsonSerializer.Serialize(new { completion = content }));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ConnectorResult.Fail($"AI provider request error: {ex.Message}");
        }
    }

    private record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] ChatMessage[] Messages);

    private record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record ChatCompletionResponse([property: JsonPropertyName("choices")] List<ChatChoice> Choices);
    private record ChatChoice([property: JsonPropertyName("message")] ChatMessage Message);
}
