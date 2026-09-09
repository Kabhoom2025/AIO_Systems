using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IContactRepository
{
    Task<List<Contact>> GetAllByOrgAsync(int orgId);
    Task<Contact?> GetByIdAsync(int orgId, int id);
    Task<Contact?> GetByUserIdAsync(int orgId, int userId);
    void Add(Contact contact);
    void Update(Contact contact);
    void Remove(Contact contact);
    Task SaveChangesAsync();
}
