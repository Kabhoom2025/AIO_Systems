namespace Chatbot.Application.Common.Interfaces;

/// <summary>
/// Builds an <see cref="IAiChatService"/> for a single call, optionally overriding the
/// provider and/or API key (e.g. a user's own "bring your own key" settings) on top of the
/// server-configured default. Keeps the provider-swap logic out of Application — this
/// interface is provider-agnostic; Infrastructure knows how "Grok"/"OpenAI"/"AzureOpenAI"
/// map to concrete implementations.
/// </summary>
public interface IAiChatServiceFactory
{
    IAiChatService Create(string? providerOverride, string? apiKeyOverride, string? modelOverride = null);
}
