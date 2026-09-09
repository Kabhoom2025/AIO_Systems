using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IPayrollRepository
{
    // Salary components
    Task<List<SalaryComponent>> GetComponentsByOrgAsync(int orgId);
    Task<SalaryComponent?> GetComponentByIdAsync(int orgId, int id);
    Task<List<SalaryComponent>> GetComponentsByIdsAsync(int orgId, List<int> ids);
    Task<bool> IsComponentUsedAsync(int componentId);
    void AddComponent(SalaryComponent component);
    void UpdateComponent(SalaryComponent component);
    void RemoveComponent(SalaryComponent component);

    // Employees
    Task<Employee?> GetEmployeeAsync(int orgId, int employeeId);

    // Employee salaries
    Task<EmployeeSalary?> GetLatestSalaryAsync(int orgId, int employeeId);
    Task<List<EmployeeSalary>> GetSalaryHistoryAsync(int orgId, int employeeId);

    /// <summary>Latest salary version (EffectiveFrom &lt;= asOf) per active employee in the org.</summary>
    Task<List<EmployeeSalary>> GetActiveLatestSalariesAsOfAsync(int orgId, DateOnly asOf);
    void AddSalary(EmployeeSalary salary);

    // Payroll runs
    Task<List<PayrollRun>> GetRunsByOrgAsync(int orgId);
    Task<PayrollRun?> GetRunByIdAsync(int orgId, int id);
    Task<bool> RunExistsAsync(int orgId, int year, int month);
    void AddRun(PayrollRun run);
    void RemoveRun(PayrollRun run);

    // Payslips
    Task<List<Payslip>> GetPayslipsByRunAsync(int orgId, int runId);
    Task<Payslip?> GetPayslipByIdAsync(int orgId, int id);
    Task<List<Payslip>> GetPayslipsByEmployeeAsync(int orgId, int employeeId);
    void AddPayslip(Payslip payslip);

    /// <summary>Sum of Days for approved leave requests of the given paid-ness overlapping the month, clipped to the month range.</summary>
    Task<decimal> GetApprovedLeaveDaysAsync(int orgId, int employeeId, int year, int month, bool isPaid);

    Task SaveChangesAsync();
}
