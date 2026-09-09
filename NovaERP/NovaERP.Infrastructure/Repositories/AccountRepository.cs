using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly NovaErpDbContext _ctx;

    public AccountRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Account>> GetAllByOrgAsync(int orgId) =>
        _ctx.Accounts
            .Include(a => a.Owner)
            .Where(a => a.OrganizationId == orgId)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public Task<Account?> GetByIdAsync(int orgId, int id) =>
        _ctx.Accounts
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == orgId);

    public void Add(Account account)    => _ctx.Accounts.Add(account);
    public void Update(Account account) => _ctx.Accounts.Update(account);
    public void Remove(Account account) => _ctx.Accounts.Remove(account);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
