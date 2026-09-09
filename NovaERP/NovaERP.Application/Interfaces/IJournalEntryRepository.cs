using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IJournalEntryRepository
{
    Task<List<JournalEntry>> GetAllByOrgAsync(int orgId);
    Task<JournalEntry?> GetByIdAsync(int orgId, int id);

    /// <summary>Posted JournalEntryLines for the given LedgerAccount not yet referenced by any
    /// BankStatementLine.MatchedJournalEntryLineId — candidates for bank reconciliation matching.</summary>
    Task<List<JournalEntryLine>> GetUnmatchedPostedLinesByAccountAsync(int orgId, int ledgerAccountId);

    /// <summary>A single JournalEntryLine by Id, scoped to the org — used to validate a match
    /// target without loading its whole parent JournalEntry graph via GetByIdAsync.</summary>
    Task<JournalEntryLine?> GetLineByIdAsync(int orgId, int journalEntryLineId);

    void Add(JournalEntry entry);
    void Update(JournalEntry entry);
    void Remove(JournalEntry entry);
    Task SaveChangesAsync();
}
