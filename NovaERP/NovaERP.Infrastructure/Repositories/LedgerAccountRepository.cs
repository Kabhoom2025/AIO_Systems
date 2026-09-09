using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class LedgerAccountRepository : ILedgerAccountRepository
{
    private readonly NovaErpDbContext _ctx;

    public LedgerAccountRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<LedgerAccount>> GetAllByOrgAsync(int orgId) =>
        _ctx.LedgerAccounts
            .Where(a => a.OrganizationId == orgId)
            .OrderBy(a => a.Code)
            .ToListAsync();

    public Task<LedgerAccount?> GetByIdAsync(int orgId, int id) =>
        _ctx.LedgerAccounts.FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.LedgerAccounts.AnyAsync(a => a.OrganizationId == orgId && a.Code == code);

    public async Task<Dictionary<int, decimal>> GetBalancesByOrgAsync(int orgId) =>
        await _ctx.JournalEntryLines
            .Where(l => l.JournalEntry.OrganizationId == orgId && l.JournalEntry.Status == "Posted")
            .GroupBy(l => l.LedgerAccountId)
            .Select(g => new { LedgerAccountId = g.Key, Balance = g.Sum(l => l.Debit - l.Credit) })
            .ToDictionaryAsync(x => x.LedgerAccountId, x => x.Balance);

    public Task<decimal> GetBalanceAsync(int orgId, int accountId) =>
        _ctx.JournalEntryLines
            .Where(l => l.LedgerAccountId == accountId && l.JournalEntry.OrganizationId == orgId && l.JournalEntry.Status == "Posted")
            .SumAsync(l => l.Debit - l.Credit);

    public void Add(LedgerAccount account)    => _ctx.LedgerAccounts.Add(account);
    public void Update(LedgerAccount account) => _ctx.LedgerAccounts.Update(account);
    public void Remove(LedgerAccount account) => _ctx.LedgerAccounts.Remove(account);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
