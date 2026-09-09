using Chatbot.Domain.Entities;

namespace Chatbot.Application.Common.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Message>> GetForConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<List<Message>> GetRecentHistoryAsync(Guid conversationId, int maxMessages, CancellationToken cancellationToken = default);
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    void Remove(Message message);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
