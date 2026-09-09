namespace Chatbot.Infrastructure.Ai;

/// <summary>
/// AI provider configuration, bound from the "AI" configuration section. Populate ApiKey via
/// environment variables / user-secrets / a secret manager — never commit it to source control.
/// Supported Provider values: "OpenAI", "AzureOpenAI", "Grok" (xAI), "Groq" (the fast-inference
/// company — a different provider from "Grok"/xAI despite the similar name; keys look like
/// "gsk_..." vs Grok's "xai-..."). Any other OpenAI-compatible provider (Together, a local
/// Ollama/vLLM server, ...) can be used by setting Provider = "OpenAI" and pointing BaseUrl at
/// that provider's endpoint.
/// </summary>
public class AiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Override the provider's default base URL (useful for OpenAI-compatible third-party/self-hosted APIs).</summary>
    public string? BaseUrl { get; set; }

    public double Temperature { get; set; } = 0.7;
    public int MaxOutputTokens { get; set; } = 1024;

    // Azure OpenAI specific
    public string? AzureEndpoint { get; set; }
    public string? AzureDeploymentName { get; set; }
    public string AzureApiVersion { get; set; } = "2024-10-21";
}
