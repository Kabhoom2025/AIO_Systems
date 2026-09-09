using Chatbot.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chatbot.Infrastructure.Ai;

/// <summary>
/// Builds an <see cref="IAiChatService"/> per call, layering an optional provider/API-key
/// override (e.g. a user's "bring your own key" settings) on top of the server-configured
/// defaults. All three providers share the same <see cref="OpenAiCompatibleChatServiceBase"/>
/// wire format, so overriding is just a matter of picking the right concrete class and handing
/// it an <see cref="AiOptions"/> with the override values merged in.
/// </summary>
public class AiChatServiceFactory(
    IHttpClientFactory httpClientFactory,
    IOptions<AiOptions> defaultOptions,
    ILoggerFactory loggerFactory) : IAiChatServiceFactory
{
    /// <summary>
    /// Sensible default chat model per provider, used when a user overrides the provider
    /// (a "bring your own key" switch to a different service) without also specifying a
    /// model — the server's configured default Model is meaningless once the provider
    /// changes, since model catalogs/names are provider-specific (e.g. "gpt-4o-mini" doesn't
    /// exist on Groq).
    /// </summary>
    private static readonly Dictionary<string, string> DefaultModelByProvider = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OpenAI"] = "gpt-4o-mini",
        ["Grok"] = "grok-2-latest",
        // Groq's model catalog rotates fairly often (models get added/retired); this is only a
        // fallback for when the user hasn't picked one themselves via modelOverride.
        ["Groq"] = "openai/gpt-oss-20b"
    };

    public IAiChatService Create(string? providerOverride, string? apiKeyOverride, string? modelOverride = null)
    {
        var defaults = defaultOptions.Value;
        var provider = string.IsNullOrWhiteSpace(providerOverride) ? defaults.Provider : providerOverride;
        var isProviderSwitch = !string.IsNullOrWhiteSpace(providerOverride)
            && !provider.Equals(defaults.Provider, StringComparison.OrdinalIgnoreCase);

        string model;
        if (!string.IsNullOrWhiteSpace(modelOverride))
        {
            model = modelOverride;
        }
        else if (isProviderSwitch && DefaultModelByProvider.TryGetValue(provider, out var providerDefaultModel))
        {
            model = providerDefaultModel;
        }
        else
        {
            model = defaults.Model;
        }

        var effective = Options.Create(new AiOptions
        {
            Provider = provider,
            ApiKey = string.IsNullOrWhiteSpace(apiKeyOverride) ? defaults.ApiKey : apiKeyOverride,
            Model = model,
            // A BaseUrl override on the server default is specific to that provider's endpoint
            // (e.g. a self-hosted OpenAI-compatible server) — it must not leak into a user's
            // switch to a different provider, which has its own well-known default endpoint.
            BaseUrl = isProviderSwitch ? null : defaults.BaseUrl,
            Temperature = defaults.Temperature,
            MaxOutputTokens = defaults.MaxOutputTokens,
            AzureEndpoint = defaults.AzureEndpoint,
            AzureDeploymentName = defaults.AzureDeploymentName,
            AzureApiVersion = defaults.AzureApiVersion
        });

        return provider.Trim().ToLowerInvariant() switch
        {
            "azureopenai" or "azure" => new AzureOpenAiChatService(
                httpClientFactory.CreateClient(nameof(AzureOpenAiChatService)), effective, loggerFactory.CreateLogger<AzureOpenAiChatService>()),
            "grok" or "xai" => new GrokChatService(
                httpClientFactory.CreateClient(nameof(GrokChatService)), effective, loggerFactory.CreateLogger<GrokChatService>()),
            "groq" => new GroqChatService(
                httpClientFactory.CreateClient(nameof(GroqChatService)), effective, loggerFactory.CreateLogger<GroqChatService>()),
            _ => new OpenAiChatService(
                httpClientFactory.CreateClient(nameof(OpenAiChatService)), effective, loggerFactory.CreateLogger<OpenAiChatService>())
        };
    }
}
