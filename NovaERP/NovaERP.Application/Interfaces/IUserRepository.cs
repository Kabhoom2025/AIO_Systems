using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllByOrgAsync(int orgId);
    Task<User?> GetByIdAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
    void Add(User user);
    void Update(User user);
    void Remove(User user);
    Task SaveChangesAsync();
}
