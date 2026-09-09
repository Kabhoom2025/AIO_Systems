using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly NovaErpDbContext _ctx;

    public JournalEntryRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<JournalEntry> Query() =>
        _ctx.JournalEntries
            .Include(e => e.Owner)
            .Include(e => e.Lines).ThenInclude(l => l.LedgerAccount);

    public Task<List<JournalEntry>> GetAllByOrgAsync(int orgId) =>
        Query().Where(e => e.OrganizationId == orgId).OrderByDescending(e => e.EntryDate).ToListAsync();

    public Task<JournalEntry?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == orgId);

    public async Task<List<JournalEntryLine>> GetUnmatchedPostedLinesByAccountAsync(int orgId, int ledgerAccountId)
    {
        var matchedLineIds = _ctx.BankStatementLines
            .Where(l => l.MatchedJournalEntryLineId != null)
            .Select(l => l.MatchedJournalEntryLineId!.Value);

        return await _ctx.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.LedgerAccountId == ledgerAccountId
                     && l.JournalEntry.OrganizationId == orgId
                     && l.JournalEntry.Status == "Posted"
                     && !matchedLineIds.Contains(l.Id))
            .OrderByDescending(l => l.JournalEntry.EntryDate)
            .ToListAsync();
    }

    public Task<JournalEntryLine?> GetLineByIdAsync(int orgId, int journalEntryLineId) =>
        _ctx.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Include(l => l.LedgerAccount)
            .FirstOrDefaultAsync(l => l.Id == journalEntryLineId && l.JournalEntry.OrganizationId == orgId);

    public void Add(JournalEntry entry)    => _ctx.JournalEntries.Add(entry);
    public void Update(JournalEntry entry) => _ctx.JournalEntries.Update(entry);
    public void Remove(JournalEntry entry) => _ctx.JournalEntries.Remove(entry);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
