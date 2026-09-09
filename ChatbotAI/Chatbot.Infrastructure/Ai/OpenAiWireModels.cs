using System.Text.Json.Serialization;

namespace Chatbot.Infrastructure.Ai;

// Wire-format models shared by every OpenAI-compatible provider (OpenAI, Azure OpenAI, Grok/xAI, ...).

internal record OpenAiMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal record OpenAiChatRequest(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("messages")] List<OpenAiMessage> Messages,
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("max_tokens")] int MaxTokens,
    [property: JsonPropertyName("stream")] bool Stream);

internal record OpenAiChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] OpenAiMessage? Message,
    [property: JsonPropertyName("delta")] OpenAiDelta? Delta,
    [property: JsonPropertyName("finish_reason")] string? FinishReason);

internal record OpenAiDelta(
    [property: JsonPropertyName("content")] string? Content);

internal record OpenAiUsage(
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens);

internal record OpenAiChatResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("choices")] List<OpenAiChoice> Choices,
    [property: JsonPropertyName("usage")] OpenAiUsage? Usage);

internal record OpenAiErrorEnvelope(
    [property: JsonPropertyName("error")] OpenAiErrorDetail? Error);

internal record OpenAiErrorDetail(
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("type")] string? Type);
