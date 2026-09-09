using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatbot.Infrastructure.Ai;

/// <summary>
/// Base implementation for any provider that speaks the OpenAI chat-completions wire format
/// (OpenAI itself, Azure OpenAI, Grok/xAI, and most self-hosted/third-party "OpenAI-compatible" APIs).
/// Concrete providers only need to supply the request URI and the auth headers.
/// </summary>
public abstract class OpenAiCompatibleChatServiceBase(
    HttpClient httpClient,
    IOptions<AiOptions> options,
    ILogger logger) : IAiChatService
{
    protected readonly AiOptions Options = options.Value;

    public abstract string ProviderName { get; }

    protected abstract string BuildRequestUri();

    protected abstract void ApplyAuthHeaders(HttpRequestMessage request);

    public async Task<AiResponse> GetResponseAsync(
        string message,
        IReadOnlyCollection<ChatMessage> history,
        CancellationToken cancellationToken)
    {
        var request = BuildRequest(history, message, stream: false);

        using var httpRequest = CreateHttpRequest(request);
        using var httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            await ThrowForErrorResponseAsync(httpResponse, cancellationToken);
        }

        var body = await httpResponse.Content.ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken: cancellationToken)
            ?? throw new AiServiceException($"{ProviderName} returned an empty response.");

        var content = body.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;

        return new AiResponse(content, body.Usage?.PromptTokens, body.Usage?.CompletionTokens, body.Model ?? Options.Model);
    }

    public async IAsyncEnumerable<AiResponseChunk> StreamResponseAsync(
        string message,
        IReadOnlyCollection<ChatMessage> history,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = BuildRequest(history, message, stream: true);

        using var httpRequest = CreateHttpRequest(request);
        using var httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            await ThrowForErrorResponseAsync(httpResponse, cancellationToken);
        }

        await using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? model = Options.Model;
        int? promptTokens = null;
        int? completionTokens = null;

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var payload = line["data:".Length..].Trim();
            if (payload == "[DONE]")
            {
                break;
            }

            OpenAiChatResponse? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<OpenAiChatResponse>(payload);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Skipping malformed SSE chunk from {Provider}", ProviderName);
                continue;
            }

            if (chunk is null)
            {
                continue;
            }

            model = chunk.Model ?? model;
            if (chunk.Usage is not null)
            {
                promptTokens = chunk.Usage.PromptTokens ?? promptTokens;
                completionTokens = chunk.Usage.CompletionTokens ?? completionTokens;
            }

            var delta = chunk.Choices.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(delta))
            {
                yield return new AiResponseChunk(delta, IsFinal: false);
            }
        }

        yield return new AiResponseChunk(string.Empty, IsFinal: true, promptTokens, completionTokens, model);
    }

    private OpenAiChatRequest BuildRequest(IReadOnlyCollection<ChatMessage> history, string message, bool stream)
    {
        var messages = history
            .Select(m => new OpenAiMessage(m.Role, m.Content))
            .ToList();

        if (!string.IsNullOrEmpty(message) && (messages.Count == 0 || messages[^1].Content != message))
        {
            messages.Add(new OpenAiMessage("user", message));
        }

        return new OpenAiChatRequest(Options.Model, messages, Options.Temperature, Options.MaxOutputTokens, stream);
    }

    private HttpRequestMessage CreateHttpRequest(OpenAiChatRequest body)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri())
        {
            Content = JsonContent.Create(body)
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        ApplyAuthHeaders(httpRequest);
        return httpRequest;
    }

    private async Task ThrowForErrorResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        string? providerMessage = null;
        try
        {
            var envelope = JsonSerializer.Deserialize<OpenAiErrorEnvelope>(rawBody);
            providerMessage = envelope?.Error?.Message;
        }
        catch (JsonException)
        {
            // fall through with generic message
        }

        logger.LogError(
            "AI provider {Provider} returned {StatusCode}: {Body}",
            ProviderName, (int)response.StatusCode, rawBody);

        throw new AiServiceException(providerMessage ?? $"{ProviderName} request failed with status {(int)response.StatusCode}.");
    }
}
