using Chatbot.Application.Common.Exceptions;
using Chatbot.Application.Common.Interfaces;
using Chatbot.Application.Messages;
using Chatbot.Domain.Entities;

namespace Chatbot.Application.Conversations;

public class ConversationService(IConversationRepository conversationRepository) : IConversationService
{
    public async Task<List<ConversationDto>> GetForUserAsync(Guid userId, string? search, CancellationToken cancellationToken = default)
    {
        var conversations = await conversationRepository.GetForUserAsync(userId, search, cancellationToken);
        return conversations
            .OrderByDescending(c => c.UpdatedAt)
            .Select(ToDto)
            .ToList();
    }

    public async Task<ConversationDetailDto> GetDetailAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetWithMessagesAsync(conversationId, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        return new ConversationDetailDto(
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.IsArchived,
            conversation.Messages
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageDto(m.Id, m.ConversationId, m.Role.ToString(), m.Content, m.CreatedAt, m.TokenCount, m.Model))
                .ToList());
    }

    public async Task<ConversationDto> CreateAsync(Guid userId, CreateConversationRequest request, CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation
        {
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "New Chat" : request.Title.Trim()
        };

        await conversationRepository.AddAsync(conversation, cancellationToken);
        await conversationRepository.SaveChangesAsync(cancellationToken);

        return ToDto(conversation);
    }

    public async Task<ConversationDto> UpdateAsync(Guid userId, Guid conversationId, UpdateConversationRequest request, CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetByIdAsync(conversationId, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            conversation.Title = request.Title.Trim();
        }

        if (request.IsArchived.HasValue)
        {
            conversation.IsArchived = request.IsArchived.Value;
        }

        conversation.UpdatedAt = DateTime.UtcNow;
        await conversationRepository.SaveChangesAsync(cancellationToken);

        return ToDto(conversation);
    }

    public async Task DeleteAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetByIdAsync(conversationId, userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Conversation), conversationId);

        conversationRepository.Remove(conversation);
        await conversationRepository.SaveChangesAsync(cancellationToken);
    }

    private static ConversationDto ToDto(Conversation conversation) => new(
        conversation.Id,
        conversation.Title,
        conversation.CreatedAt,
        conversation.UpdatedAt,
        conversation.IsArchived,
        conversation.Messages?.Count(m => !m.IsDeleted) ?? 0);
}
