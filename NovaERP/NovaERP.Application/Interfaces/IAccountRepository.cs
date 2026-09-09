using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IAccountRepository
{
    Task<List<Account>> GetAllByOrgAsync(int orgId);
    Task<Account?> GetByIdAsync(int orgId, int id);
    void Add(Account account);
    void Update(Account account);
    void Remove(Account account);
    Task SaveChangesAsync();
}
