using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Messages;

namespace Chatbot.Application.Chat;

public interface IChatOrchestrationService
{
    /// <summary>Creates/loads the conversation, persists the user's message, and returns the AI context.</summary>
    Task<PreparedTurn> PrepareTurnAsync(Guid userId, Guid? conversationId, string message, CancellationToken cancellationToken = default);

    /// <summary>Marks a previous assistant message as deleted and returns the AI context to regenerate it.</summary>
    Task<PreparedTurn> PrepareRegenerateAsync(Guid userId, Guid assistantMessageId, CancellationToken cancellationToken = default);

    /// <summary>Persists the completed assistant response and updates the conversation's timestamp/title.</summary>
    Task<MessageDto> CompleteAssistantMessageAsync(Guid conversationId, string content, string model, int? promptTokens, int? completionTokens, CancellationToken cancellationToken = default);

    /// <summary>Non-streaming convenience path: prepares the turn, calls the AI provider once, and persists the result.</summary>
    Task<ChatTurnResult> SendMessageAsync(Guid userId, SendMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>Resolves the AI provider/key to use for this user's next call — their own
    /// "bring your own key" settings if configured, otherwise the server default.</summary>
    Task<IAiChatService> ResolveAiServiceAsync(Guid userId, CancellationToken cancellationToken = default);
}
