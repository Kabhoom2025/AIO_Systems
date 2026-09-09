using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly NovaErpDbContext _ctx;

    public UserRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<User>> GetAllByOrgAsync(int orgId) =>
        _ctx.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Where(u => u.OrganizationId == orgId)
            .OrderBy(u => u.Name)
            .ToListAsync();

    public Task<User?> GetByIdAsync(int id) =>
        _ctx.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);

    public Task<bool> ExistsByEmailAsync(string email) =>
        _ctx.Users.AnyAsync(u => u.Email == email);

    public void Add(User user)    => _ctx.Users.Add(user);
    public void Update(User user) => _ctx.Users.Update(user);
    public void Remove(User user) => _ctx.Users.Remove(user);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
