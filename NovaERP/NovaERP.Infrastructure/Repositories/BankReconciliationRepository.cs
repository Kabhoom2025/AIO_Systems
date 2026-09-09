using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class BankReconciliationRepository : IBankReconciliationRepository
{
    private readonly NovaErpDbContext _ctx;

    public BankReconciliationRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<BankReconciliation> Query() =>
        _ctx.BankReconciliations
            .Include(r => r.LedgerAccount)
            .Include(r => r.Owner)
            .Include(r => r.Lines).ThenInclude(l => l.MatchedJournalEntryLine).ThenInclude(l => l!.JournalEntry);

    public Task<List<BankReconciliation>> GetAllByOrgAsync(int orgId) =>
        Query().Where(r => r.OrganizationId == orgId).OrderByDescending(r => r.StatementDate).ToListAsync();

    public Task<BankReconciliation?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(r => r.Id == id && r.OrganizationId == orgId);

    public void Add(BankReconciliation reconciliation)    => _ctx.BankReconciliations.Add(reconciliation);
    public void Update(BankReconciliation reconciliation) => _ctx.BankReconciliations.Update(reconciliation);
    public void Remove(BankReconciliation reconciliation) => _ctx.BankReconciliations.Remove(reconciliation);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
