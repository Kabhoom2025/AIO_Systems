using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IConversationRepository
{
    Task<List<Conversation>> GetAllByUserAsync(int orgId, int userId);
    Task<Conversation?> GetByIdAsync(int orgId, int id);

    void Add(Conversation conversation);
    void Remove(Conversation conversation);
    void AddMessage(Message message);
    Task SaveChangesAsync();
}
