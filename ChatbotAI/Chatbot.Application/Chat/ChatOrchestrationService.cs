using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Common.Models;
using Chatbot.Application.Messages;
using Chatbot.Domain.Entities;
using Chatbot.Domain.Enums;
using Microsoft.Extensions.Options;

namespace Chatbot.Application.Chat;

public class ChatOrchestrationService(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IUserSettingsRepository userSettingsRepository,
    IAiChatServiceFactory aiChatServiceFactory,
    IApiKeyProtector apiKeyProtector,
    IOptions<ChatOptions> chatOptions) : IChatOrchestrationService
{
    private readonly ChatOptions _options = chatOptions.Value;

    public async Task<PreparedTurn> PrepareTurnAsync(Guid userId, Guid? conversationId, string message, CancellationToken cancellationToken = default)
    {
        var conversation = conversationId.HasValue
            ? await conversationRepository.GetByIdAsync(conversationId.Value, userId, cancellationToken)
                ?? throw new NotFoundException(nameof(Conversation), conversationId.Value)
            : null;

        if (conversation is null)
        {
            conversation = new Conversation { UserId = userId, Title = BuildTitle(message) };
            await conversationRepository.AddAsync(conversation, cancellationToken);
            await conversationRepository.SaveChangesAsync(cancellationToken);
        }

        var userMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = message
        };
        await messageRepository.AddAsync(userMessage, cancellationToken);

        conversation.UpdatedAt = DateTime.UtcNow;
        await messageRepository.SaveChangesAsync(cancellationToken);

        // Fetched *after* saving the user message, so it's included as the most recent turn —
        // otherwise the AI never sees the message it's supposed to be replying to and instead
        // responds to context ending one turn earlier (it looks like it's "replying to the
        // previous message").
        var history = await messageRepository.GetRecentHistoryAsync(conversation.Id, _options.MaxHistoryMessages, cancellationToken);
        var aiHistory = BuildAiHistory(history);

        return new PreparedTurn(
            conversation.Id,
            new MessageDto(userMessage.Id, conversation.Id, userMessage.Role.ToString(), userMessage.Content, userMessage.CreatedAt, null, null),
            aiHistory);
    }

    public async Task<PreparedTurn> PrepareRegenerateAsync(Guid userId, Guid assistantMessageId, CancellationToken cancellationToken = default)
    {
        var assistantMessage = await messageRepository.GetByIdAsync(assistantMessageId, cancellationToken);
        if (assistantMessage is null || assistantMessage.Conversation is null || assistantMessage.Conversation.UserId != userId
            || assistantMessage.Role != MessageRole.Assistant)
        {
            throw new NotFoundException(nameof(Message), assistantMessageId);
        }

        var conversationId = assistantMessage.ConversationId;
        var allHistory = await messageRepository.GetRecentHistoryAsync(conversationId, _options.MaxHistoryMessages + 1, cancellationToken);
        var priorHistory = allHistory
            .Where(m => m.CreatedAt < assistantMessage.CreatedAt)
            .TakeLast(_options.MaxHistoryMessages)
            .ToList();

        assistantMessage.IsDeleted = true;
        await messageRepository.SaveChangesAsync(cancellationToken);

        return new PreparedTurn(conversationId, null, BuildAiHistory(priorHistory));
    }

    public async Task<MessageDto> CompleteAssistantMessageAsync(Guid conversationId, string content, string model, int? promptTokens, int? completionTokens, CancellationToken cancellationToken = default)
    {
        var assistantMessage = new Message
        {
            ConversationId = conversationId,
            Role = MessageRole.Assistant,
            Content = content,
            Model = model,
            TokenCount = (promptTokens ?? 0) + (completionTokens ?? 0)
        };

        await messageRepository.AddAsync(assistantMessage, cancellationToken);
        await messageRepository.SaveChangesAsync(cancellationToken);

        return new MessageDto(assistantMessage.Id, conversationId, assistantMessage.Role.ToString(), assistantMessage.Content, assistantMessage.CreatedAt, assistantMessage.TokenCount, assistantMessage.Model);
    }

    public async Task<ChatTurnResult> SendMessageAsync(Guid userId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var prepared = await PrepareTurnAsync(userId, request.ConversationId, request.Message, cancellationToken);
        var aiService = await ResolveAiServiceAsync(userId, cancellationToken);
        var aiResponse = await aiService.GetResponseAsync(request.Message, prepared.History, cancellationToken);
        var assistantMessage = await CompleteAssistantMessageAsync(
            prepared.ConversationId, aiResponse.Content, aiResponse.Model, aiResponse.PromptTokens, aiResponse.CompletionTokens, cancellationToken);

        return new ChatTurnResult(prepared.ConversationId, prepared.UserMessage!, assistantMessage);
    }

    public async Task<IAiChatService> ResolveAiServiceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
        var apiKeyOverride = string.IsNullOrEmpty(settings?.AiApiKeyEncrypted)
            ? null
            : apiKeyProtector.Unprotect(settings.AiApiKeyEncrypted);

        return aiChatServiceFactory.Create(settings?.AiProvider, apiKeyOverride, settings?.AiModel);
    }

    private List<ChatMessage> BuildAiHistory(List<Message> history)
    {
        var result = new List<ChatMessage> { new("system", _options.SystemPrompt) };
        result.AddRange(history
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(m.Role == MessageRole.Assistant ? "assistant" : "user", m.Content)));
        return result;
    }

    private static string BuildTitle(string message)
    {
        var trimmed = message.Trim();
        return trimmed.Length <= 60 ? trimmed : string.Concat(trimmed.AsSpan(0, 57), "...");
    }
}
