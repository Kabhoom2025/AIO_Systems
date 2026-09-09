using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IBankReconciliationRepository
{
    Task<List<BankReconciliation>> GetAllByOrgAsync(int orgId);
    Task<BankReconciliation?> GetByIdAsync(int orgId, int id);
    void Add(BankReconciliation reconciliation);
    void Update(BankReconciliation reconciliation);
    void Remove(BankReconciliation reconciliation);
    Task SaveChangesAsync();
}
