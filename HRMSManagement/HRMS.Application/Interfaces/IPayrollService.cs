using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IPayrollService
{
    // Salary components
    Task<List<SalaryComponentDto>> GetComponentsAsync(int orgId);
    Task<SalaryComponentDto> CreateComponentAsync(int orgId, CreateSalaryComponentDto dto);
    Task<SalaryComponentDto> UpdateComponentAsync(int orgId, int id, UpdateSalaryComponentDto dto);
    Task DeleteComponentAsync(int orgId, int id);

    // Employee salaries
    Task<EmployeeSalaryDto?> GetLatestSalaryAsync(int orgId, int employeeId);
    Task<List<EmployeeSalaryDto>> GetSalaryHistoryAsync(int orgId, int employeeId);
    Task<EmployeeSalaryDto> SetSalaryAsync(int orgId, SetEmployeeSalaryDto dto);

    // Payroll runs
    Task<List<PayrollRunDto>> GetRunsAsync(int orgId);
    Task<(PayrollRunDto Run, List<PayslipDto> Payslips)> GetRunDetailAsync(int orgId, int id);
    Task<PayrollRunDto> CreateRunAsync(int orgId, CreatePayrollRunDto dto);
    Task<PayrollRunDto> ProcessRunAsync(int orgId, int runId, int userId, string userName);
    Task<PayrollRunDto> LockRunAsync(int orgId, int runId);
    Task<PayrollRunDto> MarkPaidAsync(int orgId, int runId);
    Task DeleteRunAsync(int orgId, int runId);
    Task<(byte[] Bytes, int Year, int Month)> GetBankFileAsync(int orgId, int runId);

    // Payslips
    Task<List<PayslipDto>> GetPayslipsByRunAsync(int orgId, int runId);
    Task<PayslipDto> GetPayslipAsync(int orgId, int id);
    Task<List<PayslipDto>> GetMyPayslipsAsync(int orgId, int employeeId);
    Task<PayslipDto> GetMyPayslipAsync(int orgId, int employeeId, int id);
}
