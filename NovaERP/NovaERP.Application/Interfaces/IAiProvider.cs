using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

/// <summary>A narrow abstraction over the AI model API — currently backed by Anthropic and Groq,
/// selected per-org via AiAssistantSettings.Provider (see AiProviderRouter). Exposes at most two
/// tools per call: the always-available get_widget_data, and propose_create_service_ticket when
/// AiToolContext.CanCreateServiceTickets is true. Credentials are per-organization, stored in
/// AiAssistantSettings, so every call is scoped by orgId.</summary>
public interface IAiProvider
{
    /// <summary>False when no API key is configured for this org — callers should skip
    /// CompleteAsync entirely and return a graceful "not configured" response instead.</summary>
    Task<bool> IsConfiguredAsync(int orgId);

    Task<AiCompletionResult> CompleteAsync(int orgId, List<AiChatTurn> history, AiToolContext toolContext, CancellationToken ct = default);
}
