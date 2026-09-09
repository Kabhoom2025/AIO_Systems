using Chatbot.Application.Common.Interfaces;
using Chatbot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Infrastructure.Persistence.Repositories;

public class MessageRepository(ApplicationDbContext context) : IMessageRepository
{
    public Task<Message?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Messages.Include(m => m.Conversation).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<List<Message>> GetForConversationAsync(Guid conversationId, CancellationToken cancellationToken = default) =>
        context.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Message>> GetRecentHistoryAsync(Guid conversationId, int maxMessages, CancellationToken cancellationToken = default)
    {
        var messages = await context.Messages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Take(maxMessages)
            .ToListAsync(cancellationToken);

        messages.Reverse();
        return messages;
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default) =>
        await context.Messages.AddAsync(message, cancellationToken);

    public void Remove(Message message) => context.Messages.Remove(message);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
