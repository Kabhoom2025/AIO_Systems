using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ITicketCategoryRepository
{
    Task<List<TicketCategory>> GetAllByOrgAsync(int orgId);
    Task<TicketCategory?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(TicketCategory category);
    void Update(TicketCategory category);
    void Remove(TicketCategory category);
    Task SaveChangesAsync();
}
