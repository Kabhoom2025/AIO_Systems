using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class PayRunRepository : IPayRunRepository
{
    private readonly NovaErpDbContext _ctx;

    public PayRunRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<PayRun> Query() =>
        _ctx.PayRuns
            .Include(p => p.ExpenseLedgerAccount)
            .Include(p => p.DeductionsPayableLedgerAccount)
            .Include(p => p.Owner)
            .Include(p => p.Lines).ThenInclude(l => l.Employee);

    public Task<List<PayRun>> GetAllByOrgAsync(int orgId) =>
        Query().Where(p => p.OrganizationId == orgId)
            .OrderByDescending(p => p.PeriodYear).ThenByDescending(p => p.PeriodMonth)
            .ToListAsync();

    public Task<PayRun?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == orgId);

    public Task<List<PayRunLine>> GetPaidLinesByEmployeeIdAsync(int orgId, int employeeId) =>
        _ctx.PayRunLines
            .Include(l => l.PayRun)
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employeeId && l.PayRun.OrganizationId == orgId && l.PayRun.Status == "Paid")
            .OrderByDescending(l => l.PayRun.PeriodYear).ThenByDescending(l => l.PayRun.PeriodMonth)
            .ToListAsync();

    public void Add(PayRun payRun)    => _ctx.PayRuns.Add(payRun);
    public void Update(PayRun payRun) => _ctx.PayRuns.Update(payRun);
    public void Remove(PayRun payRun) => _ctx.PayRuns.Remove(payRun);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
