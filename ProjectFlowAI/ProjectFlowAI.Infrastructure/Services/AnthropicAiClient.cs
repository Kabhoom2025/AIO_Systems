using System.Text;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using ProjectFlowAI.Application.Interfaces;

namespace ProjectFlowAI.Infrastructure.Services;

/// <summary>Real Claude/Anthropic API integration for every /api/ai endpoint. Degrades gracefully:
/// IsConfigured is false when Ai:ApiKey is empty, and every call site (the Application-layer command
/// handlers) checks IsConfigured BEFORE attempting anything — this class also throws
/// AiNotConfiguredException defensively so a forgotten check can never reach a live SDK call with no
/// key. Uses adaptive thinking + a caller-chosen effort level per the Phase 6 spec.</summary>
public class AnthropicAiClient : IAiClient
{
    private readonly AnthropicClient? _client;
    private readonly string _model;

    public AnthropicAiClient(IConfiguration configuration)
    {
        var apiKey = configuration["Ai:ApiKey"];
        var configuredModel = configuration["Ai:Model"];
        _model = string.IsNullOrWhiteSpace(configuredModel) ? "claude-opus-4-8" : configuredModel;

        IsConfigured = !string.IsNullOrWhiteSpace(apiKey);
        _client = IsConfigured ? new AnthropicClient { ApiKey = apiKey } : null;
    }

    public bool IsConfigured { get; }

    public Task<string> CompleteAsync(string systemPrompt, string userMessage, AiEffort effort = AiEffort.Medium,
        int maxTokens = 4096, CancellationToken cancellationToken = default)
        => CompleteConversationAsync(systemPrompt, new[] { (IsUser: true, Content: userMessage) }, effort, maxTokens, cancellationToken);

    public async Task<string> CompleteConversationAsync(string systemPrompt, IReadOnlyList<(bool IsUser, string Content)> history,
        AiEffort effort = AiEffort.Medium, int maxTokens = 4096, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || _client == null)
            throw new AiNotConfiguredException();

        var messages = history.Select(h => new MessageParam
        {
            Role = h.IsUser ? Role.User : Role.Assistant,
            Content = h.Content
        }).ToList();

        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = _model,
            MaxTokens = maxTokens,
            System = systemPrompt,
            Thinking = new ThinkingConfigAdaptive(),
            OutputConfig = new OutputConfig { Effort = effort == AiEffort.High ? Effort.High : Effort.Medium },
            Messages = messages
        });

        var sb = new StringBuilder();
        foreach (var block in response.Content)
        {
            if (block.TryPickText(out TextBlock? textBlock) && textBlock != null)
                sb.Append(textBlock.Text);
        }
        return sb.ToString();
    }
}
