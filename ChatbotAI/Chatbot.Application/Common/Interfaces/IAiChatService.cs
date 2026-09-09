namespace Chatbot.Application.Common.Interfaces;

/// <summary>
/// A single turn of conversation history handed to the AI provider as context.
/// </summary>
public record ChatMessage(string Role, string Content);

public record AiResponse(string Content, int? PromptTokens, int? CompletionTokens, string Model);

/// <summary>
/// A single chunk of a streamed AI response.
/// </summary>
public record AiResponseChunk(string DeltaContent, bool IsFinal, int? PromptTokens = null, int? CompletionTokens = null, string? Model = null);

/// <summary>
/// Provider-agnostic abstraction over an LLM chat completion API. Concrete implementations
/// (OpenAI, Azure OpenAI, Grok/xAI, or any other OpenAI-compatible or custom provider) live in
/// the Infrastructure layer and are selected at startup via configuration (AI:Provider), so the
/// provider can be swapped without touching application or presentation code.
/// </summary>
public interface IAiChatService
{
    /// <summary>Name of the provider this implementation talks to (e.g. "OpenAI", "AzureOpenAI", "Grok").</summary>
    string ProviderName { get; }

    Task<AiResponse> GetResponseAsync(
        string message,
        IReadOnlyCollection<ChatMessage> history,
        CancellationToken cancellationToken);

    IAsyncEnumerable<AiResponseChunk> StreamResponseAsync(
        string message,
        IReadOnlyCollection<ChatMessage> history,
        CancellationToken cancellationToken);
}
