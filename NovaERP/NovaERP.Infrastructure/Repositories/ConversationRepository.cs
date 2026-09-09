using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly NovaErpDbContext _ctx;

    public ConversationRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Conversation> Query() =>
        _ctx.Conversations.Include(c => c.Messages);

    public Task<List<Conversation>> GetAllByUserAsync(int orgId, int userId) =>
        Query().Where(c => c.OrganizationId == orgId && c.UserId == userId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

    public Task<Conversation?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId);

    public void Add(Conversation conversation)    => _ctx.Conversations.Add(conversation);
    public void Remove(Conversation conversation) => _ctx.Conversations.Remove(conversation);
    public void AddMessage(Message message)       => _ctx.Set<Message>().Add(message);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
