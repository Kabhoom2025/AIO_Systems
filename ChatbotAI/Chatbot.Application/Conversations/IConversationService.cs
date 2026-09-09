namespace Chatbot.Application.Conversations;

public interface IConversationService
{
    Task<List<ConversationDto>> GetForUserAsync(Guid userId, string? search, CancellationToken cancellationToken = default);
    Task<ConversationDetailDto> GetDetailAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
    Task<ConversationDto> CreateAsync(Guid userId, CreateConversationRequest request, CancellationToken cancellationToken = default);
    Task<ConversationDto> UpdateAsync(Guid userId, Guid conversationId, UpdateConversationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}
