namespace ProjectFlowAI.Application.Interfaces;

/// <summary>How hard the model should think/act — mirrors the Claude API's output_config.effort.
/// Medium is used for routine generation tasks; High for analysis-heavy ones (bug analysis, risk
/// prediction, code review) per the Phase 6 spec.</summary>
public enum AiEffort
{
    Medium,
    High
}

/// <summary>Thin abstraction over the Anthropic SDK so Application-layer command/query handlers
/// never depend on the Anthropic package directly, and so unit tests can stub AI responses without
/// hitting the real API. Checked for IsConfigured BEFORE any call is attempted — see
/// AiNotConfiguredException, which every command handler throws proactively rather than letting the
/// SDK surface an opaque auth error.</summary>
public interface IAiClient
{
    /// <summary>False when Ai:ApiKey is empty/unset in configuration.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends a single-turn request (system prompt + one user message, no conversation
    /// history) and returns the concatenated text of the response. Throws AiNotConfiguredException
    /// if IsConfigured is false — callers should check IsConfigured first for a clean early-out, but
    /// this guard exists so a forgotten check can never cause a live Anthropic API call to be attempted.</summary>
    Task<string> CompleteAsync(string systemPrompt, string userMessage, AiEffort effort = AiEffort.Medium,
        int maxTokens = 4096, CancellationToken cancellationToken = default);

    /// <summary>Multi-turn variant used by /ai/chat: (role, content) history plus a system prompt.</summary>
    Task<string> CompleteConversationAsync(string systemPrompt, IReadOnlyList<(bool IsUser, string Content)> history,
        AiEffort effort = AiEffort.Medium, int maxTokens = 4096, CancellationToken cancellationToken = default);
}

/// <summary>Thrown proactively (before attempting any Anthropic API call) when Ai:ApiKey is
/// unconfigured. Mapped to 503 ProblemDetails by GlobalExceptionMiddleware — mirrors the
/// "degrade gracefully to a clear error when unconfigured" convention already used by
/// SmtpEmailSender/SlackNotifier/etc., just surfaced as an exception instead of a silent log-only no-op
/// since an AI endpoint has no sensible non-AI fallback behavior.</summary>
public class AiNotConfiguredException : Exception
{
    public AiNotConfiguredException()
        : base("AI features are not configured — set Ai:ApiKey in configuration.") { }
}
