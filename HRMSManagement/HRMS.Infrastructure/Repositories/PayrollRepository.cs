using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class PayrollRepository : IPayrollRepository
{
    private readonly HrmsDbContext _ctx;

    public PayrollRepository(HrmsDbContext ctx) => _ctx = ctx;

    // ---------- Salary components ----------

    public Task<List<SalaryComponent>> GetComponentsByOrgAsync(int orgId) =>
        _ctx.SalaryComponents
            .Where(c => c.OrganizationId == orgId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

    public Task<SalaryComponent?> GetComponentByIdAsync(int orgId, int id) =>
        _ctx.SalaryComponents.FirstOrDefaultAsync(c => c.OrganizationId == orgId && c.Id == id);

    public Task<List<SalaryComponent>> GetComponentsByIdsAsync(int orgId, List<int> ids) =>
        _ctx.SalaryComponents
            .Where(c => c.OrganizationId == orgId && ids.Contains(c.Id))
            .ToListAsync();

    public Task<bool> IsComponentUsedAsync(int componentId) =>
        _ctx.EmployeeSalaryItems.AnyAsync(i => i.SalaryComponentId == componentId);

    public void AddComponent(SalaryComponent component)    => _ctx.SalaryComponents.Add(component);
    public void UpdateComponent(SalaryComponent component) => _ctx.SalaryComponents.Update(component);
    public void RemoveComponent(SalaryComponent component) => _ctx.SalaryComponents.Remove(component);

    // ---------- Employees ----------

    public Task<Employee?> GetEmployeeAsync(int orgId, int employeeId) =>
        _ctx.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.OrganizationId == orgId && e.Id == employeeId);

    // ---------- Employee salaries ----------

    public Task<EmployeeSalary?> GetLatestSalaryAsync(int orgId, int employeeId) =>
        _ctx.EmployeeSalaries
            .Include(s => s.Employee)
            .Include(s => s.Items).ThenInclude(i => i.SalaryComponent)
            .Where(s => s.OrganizationId == orgId && s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync();

    public Task<List<EmployeeSalary>> GetSalaryHistoryAsync(int orgId, int employeeId) =>
        _ctx.EmployeeSalaries
            .Include(s => s.Employee)
            .Include(s => s.Items).ThenInclude(i => i.SalaryComponent)
            .Where(s => s.OrganizationId == orgId && s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveFrom)
            .ToListAsync();

    public async Task<List<EmployeeSalary>> GetActiveLatestSalariesAsOfAsync(int orgId, DateOnly asOf)
    {
        var all = await _ctx.EmployeeSalaries
            .Include(s => s.Employee).ThenInclude(e => e.Department)
            .Include(s => s.Employee).ThenInclude(e => e.Designation)
            .Include(s => s.Items).ThenInclude(i => i.SalaryComponent)
            .Where(s => s.OrganizationId == orgId
                     && s.EffectiveFrom <= asOf
                     && s.Employee.Status == "Active")
            .ToListAsync();

        return all
            .GroupBy(s => s.EmployeeId)
            .Select(g => g.OrderByDescending(s => s.EffectiveFrom).First())
            .ToList();
    }

    public void AddSalary(EmployeeSalary salary) => _ctx.EmployeeSalaries.Add(salary);

    // ---------- Payroll runs ----------

    public Task<List<PayrollRun>> GetRunsByOrgAsync(int orgId) =>
        _ctx.PayrollRuns
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Payslips)
            .Where(r => r.OrganizationId == orgId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .ToListAsync();

    public Task<PayrollRun?> GetRunByIdAsync(int orgId, int id) =>
        _ctx.PayrollRuns
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Payslips).ThenInclude(p => p.Items)
            .Include(r => r.Payslips).ThenInclude(p => p.Employee).ThenInclude(e => e.Department)
            .Include(r => r.Payslips).ThenInclude(p => p.Employee).ThenInclude(e => e.Designation)
            .FirstOrDefaultAsync(r => r.OrganizationId == orgId && r.Id == id);

    public Task<bool> RunExistsAsync(int orgId, int year, int month) =>
        _ctx.PayrollRuns.AnyAsync(r => r.OrganizationId == orgId && r.Year == year && r.Month == month);

    public void AddRun(PayrollRun run)    => _ctx.PayrollRuns.Add(run);
    public void RemoveRun(PayrollRun run) => _ctx.PayrollRuns.Remove(run);

    // ---------- Payslips ----------

    public Task<List<Payslip>> GetPayslipsByRunAsync(int orgId, int runId) =>
        _ctx.Payslips
            .Include(p => p.PayrollRun)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Designation)
            .Include(p => p.Items)
            .Where(p => p.PayrollRun.OrganizationId == orgId && p.PayrollRunId == runId)
            .OrderBy(p => p.Employee.EmployeeCode)
            .ToListAsync();

    public Task<Payslip?> GetPayslipByIdAsync(int orgId, int id) =>
        _ctx.Payslips
            .Include(p => p.PayrollRun)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Designation)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PayrollRun.OrganizationId == orgId && p.Id == id);

    public Task<List<Payslip>> GetPayslipsByEmployeeAsync(int orgId, int employeeId) =>
        _ctx.Payslips
            .Include(p => p.PayrollRun)
            .Include(p => p.Employee).ThenInclude(e => e.Department)
            .Include(p => p.Employee).ThenInclude(e => e.Designation)
            .Include(p => p.Items)
            .Where(p => p.PayrollRun.OrganizationId == orgId && p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayrollRun.Year).ThenByDescending(p => p.PayrollRun.Month)
            .ToListAsync();

    public void AddPayslip(Payslip payslip) => _ctx.Payslips.Add(payslip);

    // ---------- Leave days (LOP / paid-leave) ----------

    public async Task<decimal> GetApprovedLeaveDaysAsync(int orgId, int employeeId, int year, int month, bool isPaid)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd   = monthStart.AddMonths(1).AddDays(-1);

        var requests = await _ctx.LeaveRequests
            .Include(r => r.LeaveType)
            .Where(r => r.OrganizationId == orgId
                     && r.EmployeeId == employeeId
                     && r.Status == "Approved"
                     && r.LeaveType.IsPaid == isPaid
                     && r.StartDate <= monthEnd
                     && r.EndDate >= monthStart)
            .ToListAsync();

        decimal total = 0;
        foreach (var r in requests)
        {
            if (r.StartDate >= monthStart && r.EndDate <= monthEnd)
            {
                // Entirely within the month — use the recorded Days value (handles half-days correctly).
                total += r.Days;
            }
            else
            {
                // Spans the month boundary — clip to the overlapping calendar days.
                var overlapStart = r.StartDate > monthStart ? r.StartDate : monthStart;
                var overlapEnd   = r.EndDate   < monthEnd   ? r.EndDate   : monthEnd;
                total += overlapEnd.DayNumber - overlapStart.DayNumber + 1;
            }
        }
        return total;
    }

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
