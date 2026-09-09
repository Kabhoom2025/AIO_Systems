using Chatbot.Domain.Entities;

namespace Chatbot.Application.Common.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Conversation?> GetWithMessagesAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetForUserAsync(Guid userId, string? search, CancellationToken cancellationToken = default);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    void Remove(Conversation conversation);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
