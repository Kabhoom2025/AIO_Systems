using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IPayRunRepository
{
    Task<List<PayRun>> GetAllByOrgAsync(int orgId);
    Task<PayRun?> GetByIdAsync(int orgId, int id);

    /// <summary>PayRunLines for the given employee, scoped to Paid runs only — a
    /// Processed-but-unpaid run is an internal draft, not a real payslip yet.</summary>
    Task<List<PayRunLine>> GetPaidLinesByEmployeeIdAsync(int orgId, int employeeId);

    void Add(PayRun payRun);
    void Update(PayRun payRun);
    void Remove(PayRun payRun);
    Task SaveChangesAsync();
}
