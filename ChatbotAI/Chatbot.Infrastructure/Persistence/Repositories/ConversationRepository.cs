using Chatbot.Application.Common.Interfaces;
using Chatbot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chatbot.Infrastructure.Persistence.Repositories;

public class ConversationRepository(ApplicationDbContext context) : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

    public Task<Conversation?> GetWithMessagesAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

    public async Task<List<Conversation>> GetForUserAsync(Guid userId, string? search, CancellationToken cancellationToken = default)
    {
        var query = context.Conversations
            .Include(c => c.Messages)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => EF.Functions.ILike(c.Title, $"%{search}%"));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default) =>
        await context.Conversations.AddAsync(conversation, cancellationToken);

    public void Remove(Conversation conversation) => context.Conversations.Remove(conversation);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
