using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository _repo;

    public PayrollService(IPayrollRepository repo) => _repo = repo;

    // ---------- Salary components ----------

    public async Task<List<SalaryComponentDto>> GetComponentsAsync(int orgId)
    {
        var components = await _repo.GetComponentsByOrgAsync(orgId);
        return components.Select(MapComponent).ToList();
    }

    public async Task<SalaryComponentDto> CreateComponentAsync(int orgId, CreateSalaryComponentDto dto)
    {
        var component = new SalaryComponent
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            Code           = dto.Code,
            Type           = dto.Type,
            CalcType       = dto.CalcType,
            DefaultValue   = dto.DefaultValue,
            IsTaxable      = dto.IsTaxable,
            IsStatutory    = dto.IsStatutory,
            DisplayOrder   = dto.DisplayOrder,
            IsActive       = true
        };
        _repo.AddComponent(component);
        await _repo.SaveChangesAsync();
        return MapComponent(component);
    }

    public async Task<SalaryComponentDto> UpdateComponentAsync(int orgId, int id, UpdateSalaryComponentDto dto)
    {
        var component = await _repo.GetComponentByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Salary component {id} not found.");

        component.Name         = dto.Name;
        component.Code         = dto.Code;
        component.Type         = dto.Type;
        component.CalcType     = dto.CalcType;
        component.DefaultValue = dto.DefaultValue;
        component.IsTaxable    = dto.IsTaxable;
        component.IsStatutory  = dto.IsStatutory;
        component.DisplayOrder = dto.DisplayOrder;
        component.IsActive     = dto.IsActive;
        component.UpdatedDate  = DateTime.UtcNow;

        _repo.UpdateComponent(component);
        await _repo.SaveChangesAsync();
        return MapComponent(component);
    }

    public async Task DeleteComponentAsync(int orgId, int id)
    {
        var component = await _repo.GetComponentByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Salary component {id} not found.");

        if (await _repo.IsComponentUsedAsync(id))
            throw new InvalidOperationException("Cannot delete a salary component that is used in an employee salary structure.");

        _repo.RemoveComponent(component);
        await _repo.SaveChangesAsync();
    }

    // ---------- Employee salaries ----------

    public async Task<EmployeeSalaryDto?> GetLatestSalaryAsync(int orgId, int employeeId)
    {
        var salary = await _repo.GetLatestSalaryAsync(orgId, employeeId);
        return salary == null ? null : MapSalary(salary);
    }

    public async Task<List<EmployeeSalaryDto>> GetSalaryHistoryAsync(int orgId, int employeeId)
    {
        var salaries = await _repo.GetSalaryHistoryAsync(orgId, employeeId);
        return salaries.Select(MapSalary).ToList();
    }

    public async Task<EmployeeSalaryDto> SetSalaryAsync(int orgId, SetEmployeeSalaryDto dto)
    {
        var employee = await _repo.GetEmployeeAsync(orgId, dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found.");

        var componentIds = dto.Items.Select(i => i.SalaryComponentId).Distinct().ToList();
        var components   = await _repo.GetComponentsByIdsAsync(orgId, componentIds);
        var componentMap = components.ToDictionary(c => c.Id);

        if (componentMap.Count != componentIds.Count)
            throw new KeyNotFoundException("One or more salary components were not found.");

        var salary = new EmployeeSalary
        {
            OrganizationId = orgId,
            EmployeeId     = dto.EmployeeId,
            EffectiveFrom  = dto.EffectiveFrom,
            AnnualCtc      = dto.AnnualCtc,
            Currency       = dto.Currency,
            Notes          = dto.Notes
        };

        decimal monthlyGross = 0;
        foreach (var item in dto.Items)
        {
            var component = componentMap[item.SalaryComponentId];
            salary.Items.Add(new EmployeeSalaryItem
            {
                SalaryComponentId = item.SalaryComponentId,
                MonthlyAmount     = item.MonthlyAmount
            });
            if (component.Type == "Earning")
                monthlyGross += item.MonthlyAmount;
        }
        salary.MonthlyGross = monthlyGross;

        _repo.AddSalary(salary);
        await _repo.SaveChangesAsync();

        // Populate navigations locally so the response DTO is complete without a re-query.
        salary.Employee = employee;
        foreach (var item in salary.Items)
            item.SalaryComponent = componentMap[item.SalaryComponentId];

        return MapSalary(salary);
    }

    // ---------- Payroll runs ----------

    public async Task<List<PayrollRunDto>> GetRunsAsync(int orgId)
    {
        var runs = await _repo.GetRunsByOrgAsync(orgId);
        return runs.Select(r => MapRun(r)).ToList();
    }

    public async Task<(PayrollRunDto Run, List<PayslipDto> Payslips)> GetRunDetailAsync(int orgId, int id)
    {
        var run = await _repo.GetRunByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Payroll run {id} not found.");

        foreach (var p in run.Payslips)
            p.PayrollRun = run;

        var payslips = run.Payslips
            .OrderBy(p => p.Employee.EmployeeCode)
            .Select(MapPayslip)
            .ToList();

        return (MapRun(run), payslips);
    }

    public async Task<PayrollRunDto> CreateRunAsync(int orgId, CreatePayrollRunDto dto)
    {
        if (dto.Month is < 1 or > 12)
            throw new InvalidOperationException("Month must be between 1 and 12.");

        if (await _repo.RunExistsAsync(orgId, dto.Year, dto.Month))
            throw new InvalidOperationException("Payroll for this period already exists.");

        var run = new PayrollRun
        {
            OrganizationId = orgId,
            Year           = dto.Year,
            Month          = dto.Month,
            Status         = "Draft",
            Notes          = dto.Notes
        };
        _repo.AddRun(run);
        await _repo.SaveChangesAsync();
        return MapRun(run, payslipCountOverride: 0);
    }

    public async Task<PayrollRunDto> ProcessRunAsync(int orgId, int runId, int userId, string userName)
    {
        var run = await _repo.GetRunByIdAsync(orgId, runId)
            ?? throw new KeyNotFoundException($"Payroll run {runId} not found.");

        if (run.Status != "Draft")
            throw new InvalidOperationException("Only draft payroll runs can be processed.");

        var monthStart  = new DateOnly(run.Year, run.Month, 1);
        var monthEnd    = monthStart.AddMonths(1).AddDays(-1);
        var workingDays = DateTime.DaysInMonth(run.Year, run.Month);

        var salaries = await _repo.GetActiveLatestSalariesAsOfAsync(orgId, monthEnd);

        decimal totalGross = 0, totalDeductions = 0, totalNet = 0;

        foreach (var salary in salaries)
        {
            var lopDays       = await _repo.GetApprovedLeaveDaysAsync(orgId, salary.EmployeeId, run.Year, run.Month, isPaid: false);
            var paidLeaveDays = await _repo.GetApprovedLeaveDaysAsync(orgId, salary.EmployeeId, run.Year, run.Month, isPaid: true);
            var presentDays   = workingDays - lopDays;
            var perDay        = salary.MonthlyGross / workingDays;
            var lopDeduction  = Math.Round(perDay * lopDays, 2);

            var payslip = new Payslip
            {
                PayrollRunId  = run.Id,
                EmployeeId    = salary.EmployeeId,
                WorkingDays   = workingDays,
                PresentDays   = presentDays,
                PaidLeaveDays = paidLeaveDays,
                LopDays       = lopDays,
                Status        = "Generated"
            };

            decimal gross = 0, deductions = 0;
            foreach (var item in salary.Items)
            {
                payslip.Items.Add(new PayslipItem
                {
                    ComponentName = item.SalaryComponent.Name,
                    Type          = item.SalaryComponent.Type,
                    Amount        = item.MonthlyAmount
                });
                if (item.SalaryComponent.Type == "Earning")
                    gross += item.MonthlyAmount;
                else
                    deductions += item.MonthlyAmount;
            }

            if (lopDeduction > 0)
            {
                payslip.Items.Add(new PayslipItem
                {
                    ComponentName = "Loss of Pay",
                    Type          = "Deduction",
                    Amount        = lopDeduction
                });
                deductions += lopDeduction;
            }

            payslip.GrossEarnings   = gross;
            payslip.TotalDeductions = deductions;
            payslip.NetPay          = gross - deductions;

            _repo.AddPayslip(payslip);

            totalGross      += gross;
            totalDeductions += deductions;
            totalNet        += payslip.NetPay;
        }

        run.Status            = "Completed";
        run.ProcessedAt       = DateTime.UtcNow;
        run.ProcessedByUserId = userId;
        run.TotalGross         = totalGross;
        run.TotalDeductions    = totalDeductions;
        run.TotalNet           = totalNet;
        run.UpdatedDate        = DateTime.UtcNow;

        await _repo.SaveChangesAsync();

        return MapRun(run, payslipCountOverride: salaries.Count, processedByNameOverride: userName);
    }

    public async Task<PayrollRunDto> LockRunAsync(int orgId, int runId)
    {
        var run = await _repo.GetRunByIdAsync(orgId, runId)
            ?? throw new KeyNotFoundException($"Payroll run {runId} not found.");

        if (run.Status != "Completed")
            throw new InvalidOperationException("Only completed payroll runs can be locked.");

        run.Status      = "Locked";
        run.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
        return MapRun(run);
    }

    public async Task<PayrollRunDto> MarkPaidAsync(int orgId, int runId)
    {
        var run = await _repo.GetRunByIdAsync(orgId, runId)
            ?? throw new KeyNotFoundException($"Payroll run {runId} not found.");

        if (run.Status != "Locked")
            throw new InvalidOperationException("Only locked payroll runs can be marked as paid.");

        run.Status      = "Paid";
        run.UpdatedDate = DateTime.UtcNow;
        foreach (var payslip in run.Payslips)
            payslip.Status = "Paid";

        await _repo.SaveChangesAsync();
        return MapRun(run);
    }

    public async Task DeleteRunAsync(int orgId, int runId)
    {
        var run = await _repo.GetRunByIdAsync(orgId, runId)
            ?? throw new KeyNotFoundException($"Payroll run {runId} not found.");

        if (run.Status is "Locked" or "Paid")
            throw new InvalidOperationException("Locked or paid payroll runs cannot be deleted.");

        _repo.RemoveRun(run);
        await _repo.SaveChangesAsync();
    }

    public async Task<(byte[] Bytes, int Year, int Month)> GetBankFileAsync(int orgId, int runId)
    {
        var run = await _repo.GetRunByIdAsync(orgId, runId)
            ?? throw new KeyNotFoundException($"Payroll run {runId} not found.");

        var payslips = await _repo.GetPayslipsByRunAsync(orgId, runId);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("EmployeeCode,EmployeeName,BankName,AccountNumber,IFSC,NetPay");
        foreach (var p in payslips)
        {
            sb.AppendLine(string.Join(",",
                Csv(p.Employee.EmployeeCode),
                Csv(p.Employee.FullName),
                Csv(p.Employee.BankName ?? string.Empty),
                Csv(p.Employee.BankAccountNumber ?? string.Empty),
                Csv(p.Employee.BankIfscCode ?? string.Empty),
                p.NetPay.ToString("F2")));
        }

        return (System.Text.Encoding.UTF8.GetBytes(sb.ToString()), run.Year, run.Month);
    }

    private static string Csv(string value) => value.Contains(',') ? $"\"{value}\"" : value;

    // ---------- Payslips ----------

    public async Task<List<PayslipDto>> GetPayslipsByRunAsync(int orgId, int runId)
    {
        _ = await _repo.GetRunByIdAsync(orgId, runId)
            ?? throw new KeyNotFoundException($"Payroll run {runId} not found.");

        var payslips = await _repo.GetPayslipsByRunAsync(orgId, runId);
        return payslips.Select(MapPayslip).ToList();
    }

    public async Task<PayslipDto> GetPayslipAsync(int orgId, int id)
    {
        var payslip = await _repo.GetPayslipByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Payslip {id} not found.");
        return MapPayslip(payslip);
    }

    public async Task<List<PayslipDto>> GetMyPayslipsAsync(int orgId, int employeeId)
    {
        var payslips = await _repo.GetPayslipsByEmployeeAsync(orgId, employeeId);
        return payslips.Select(MapPayslip).ToList();
    }

    public async Task<PayslipDto> GetMyPayslipAsync(int orgId, int employeeId, int id)
    {
        var payslip = await _repo.GetPayslipByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Payslip {id} not found.");

        if (payslip.EmployeeId != employeeId)
            throw new KeyNotFoundException($"Payslip {id} not found.");

        return MapPayslip(payslip);
    }

    // ---------- Mapping ----------

    private static SalaryComponentDto MapComponent(SalaryComponent c) => new()
    {
        Id           = c.Id,
        Name         = c.Name,
        Code         = c.Code,
        Type         = c.Type,
        CalcType     = c.CalcType,
        DefaultValue = c.DefaultValue,
        IsTaxable    = c.IsTaxable,
        IsStatutory  = c.IsStatutory,
        DisplayOrder = c.DisplayOrder,
        IsActive     = c.IsActive
    };

    private static EmployeeSalaryDto MapSalary(EmployeeSalary s) => new()
    {
        Id            = s.Id,
        EmployeeId    = s.EmployeeId,
        EmployeeName  = s.Employee?.FullName ?? string.Empty,
        EffectiveFrom = s.EffectiveFrom,
        AnnualCtc     = s.AnnualCtc,
        MonthlyGross  = s.MonthlyGross,
        Currency      = s.Currency,
        Notes         = s.Notes,
        Items = s.Items.Select(i => new EmployeeSalaryItemDto
        {
            ComponentId   = i.SalaryComponentId,
            ComponentName = i.SalaryComponent?.Name ?? string.Empty,
            Type          = i.SalaryComponent?.Type ?? string.Empty,
            MonthlyAmount = i.MonthlyAmount
        }).ToList()
    };

    private static PayrollRunDto MapRun(PayrollRun r, int? payslipCountOverride = null, string? processedByNameOverride = null) => new()
    {
        Id              = r.Id,
        Year            = r.Year,
        Month           = r.Month,
        Status          = r.Status,
        ProcessedAt     = r.ProcessedAt,
        ProcessedByName = processedByNameOverride ?? r.ProcessedByUser?.Name,
        TotalGross      = r.TotalGross,
        TotalDeductions = r.TotalDeductions,
        TotalNet        = r.TotalNet,
        PayslipCount    = payslipCountOverride ?? r.Payslips.Count,
        Notes           = r.Notes
    };

    private static PayslipDto MapPayslip(Payslip p) => new()
    {
        Id               = p.Id,
        EmployeeId       = p.EmployeeId,
        EmployeeCode     = p.Employee?.EmployeeCode ?? string.Empty,
        EmployeeName     = p.Employee?.FullName ?? string.Empty,
        DepartmentName   = p.Employee?.Department?.Name ?? string.Empty,
        DesignationTitle = p.Employee?.Designation?.Title ?? string.Empty,
        Year             = p.PayrollRun?.Year ?? 0,
        Month            = p.PayrollRun?.Month ?? 0,
        GrossEarnings    = p.GrossEarnings,
        TotalDeductions  = p.TotalDeductions,
        NetPay           = p.NetPay,
        WorkingDays      = p.WorkingDays,
        PresentDays      = p.PresentDays,
        PaidLeaveDays    = p.PaidLeaveDays,
        LopDays          = p.LopDays,
        Status           = p.Status,
        Items = p.Items.Select(i => new PayslipItemDto
        {
            ComponentName = i.ComponentName,
            Type          = i.Type,
            Amount        = i.Amount
        }).ToList()
    };
}
