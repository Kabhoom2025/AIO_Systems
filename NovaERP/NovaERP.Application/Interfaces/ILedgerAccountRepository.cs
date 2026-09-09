using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ILedgerAccountRepository
{
    Task<List<LedgerAccount>> GetAllByOrgAsync(int orgId);
    Task<LedgerAccount?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    /// <summary>Sum of Debit - Credit per LedgerAccountId across Posted journal entries only,
    /// for the whole org in one query — avoids N+1 when building the account list, each row
    /// needing its own balance.</summary>
    Task<Dictionary<int, decimal>> GetBalancesByOrgAsync(int orgId);
    Task<decimal> GetBalanceAsync(int orgId, int accountId);

    void Add(LedgerAccount account);
    void Update(LedgerAccount account);
    void Remove(LedgerAccount account);
    Task SaveChangesAsync();
}
