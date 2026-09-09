using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class PayRunService : IPayRunService
{
    private readonly IPayRunRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IEmployeeCompensationRepository _compensationRepo;
    private readonly IJournalEntryRepository _journalRepo;
    private readonly IValidator<CreatePayRunDto> _createValidator;
    private readonly IValidator<UpdatePayRunDto> _updateValidator;
    private readonly IValidator<PayPayRunDto> _payValidator;

    public PayRunService(IPayRunRepository repo, IEmployeeRepository employeeRepo,
        IEmployeeCompensationRepository compensationRepo, IJournalEntryRepository journalRepo,
        IValidator<CreatePayRunDto> createValidator, IValidator<UpdatePayRunDto> updateValidator,
        IValidator<PayPayRunDto> payValidator)
    {
        _repo = repo;
        _employeeRepo = employeeRepo;
        _compensationRepo = compensationRepo;
        _journalRepo = journalRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _payValidator = payValidator;
    }

    public async Task<List<PayRunDto>> GetAllAsync(int orgId)
    {
        var runs = await _repo.GetAllByOrgAsync(orgId);
        return runs.Select(ToDto).ToList();
    }

    public async Task<PayRunDto> GetByIdAsync(int orgId, int id)
    {
        var run = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PayRun {id} not found");
        return ToDto(run);
    }

    public async Task<PayRunDto> CreateAsync(int orgId, CreatePayRunDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var run = new PayRun
        {
            OrganizationId = orgId,
            PeriodMonth = dto.PeriodMonth,
            PeriodYear = dto.PeriodYear,
            ExpenseLedgerAccountId = dto.ExpenseLedgerAccountId,
            DeductionsPayableLedgerAccountId = dto.DeductionsPayableLedgerAccountId,
            Status = "Draft",
            OwnerId = dto.OwnerId
        };

        _repo.Add(run);
        await _repo.SaveChangesAsync();

        // RunNumber depends on the generated Id, so it's set in a second save — same scheme
        // as VendorBill.BillNumber/JournalEntry.EntryNumber.
        run.RunNumber = $"PR-{run.Id:D5}";
        _repo.Update(run);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, run.Id) ?? run;
        return ToDto(reloaded);
    }

    public async Task<PayRunDto> UpdateAsync(int orgId, int id, UpdatePayRunDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var run = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PayRun {id} not found");

        if (run.Status != "Draft")
            throw new InvalidOperationException("Only draft pay runs can be edited.");

        run.PeriodMonth = dto.PeriodMonth;
        run.PeriodYear = dto.PeriodYear;
        run.ExpenseLedgerAccountId = dto.ExpenseLedgerAccountId;
        run.DeductionsPayableLedgerAccountId = dto.DeductionsPayableLedgerAccountId;
        run.OwnerId = dto.OwnerId;
        run.UpdatedDate = DateTime.UtcNow;

        _repo.Update(run);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, run.Id) ?? run;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var run = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PayRun {id} not found");

        if (run.Status != "Draft")
            throw new InvalidOperationException("Only draft pay runs can be deleted.");

        _repo.Remove(run);
        await _repo.SaveChangesAsync();
    }

    public async Task<PayRunDto> ProcessAsync(int orgId, int id)
    {
        var run = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PayRun {id} not found");

        if (run.Status != "Draft")
            throw new InvalidOperationException("Only draft pay runs can be processed.");

        var employees = await _employeeRepo.GetAllByOrgAsync(orgId);
        var compensations = await _compensationRepo.GetAllByOrgAsync(orgId);
        var compensationByEmployeeId = compensations.ToDictionary(c => c.EmployeeId);

        run.Lines.Clear();
        foreach (var employee in employees.Where(e => e.Status == "Active"))
        {
            if (!compensationByEmployeeId.TryGetValue(employee.Id, out var comp))
                continue; // no compensation on file yet — skipped, not an error

            var gross = comp.BasicSalary + comp.Hra + comp.OtherAllowances;
            run.Lines.Add(new PayRunLine
            {
                EmployeeId = employee.Id,
                BasicSalary = comp.BasicSalary,
                Hra = comp.Hra,
                OtherAllowances = comp.OtherAllowances,
                Deductions = comp.Deductions,
                GrossPay = gross,
                NetPay = gross - comp.Deductions
            });
        }

        if (!run.Lines.Any())
            throw new InvalidOperationException("No active employees with a compensation record were found to process.");

        run.Status = "Processed";
        run.UpdatedDate = DateTime.UtcNow;
        _repo.Update(run);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, run.Id) ?? run;
        return ToDto(reloaded);
    }

    public async Task<PayRunDto> PayAsync(int orgId, int id, PayPayRunDto dto)
    {
        await _payValidator.ValidateAndThrowAsync(dto);

        var run = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PayRun {id} not found");

        if (run.Status != "Processed")
            throw new InvalidOperationException("Only processed pay runs can be paid.");

        var totalGross = run.Lines.Sum(l => l.GrossPay);
        var totalDeductions = run.Lines.Sum(l => l.Deductions);
        var totalNet = run.Lines.Sum(l => l.NetPay);

        var entry = new JournalEntry
        {
            OrganizationId = orgId,
            EntryDate = DateTime.UtcNow.Date,
            Description = $"Pay Run {run.RunNumber}",
            Status = "Posted",
            OwnerId = run.OwnerId,
            Lines = new List<JournalEntryLine>
            {
                new() { LedgerAccountId = run.ExpenseLedgerAccountId, Debit = totalGross, Credit = 0m, DisplayOrder = 1 },
                new() { LedgerAccountId = run.DeductionsPayableLedgerAccountId, Debit = 0m, Credit = totalDeductions, DisplayOrder = 2 },
                new() { LedgerAccountId = dto.PaymentLedgerAccountId, Debit = 0m, Credit = totalNet, DisplayOrder = 3 }
            }
        };

        _journalRepo.Add(entry);
        await _journalRepo.SaveChangesAsync();

        entry.EntryNumber = $"JE-{entry.Id:D5}";
        _journalRepo.Update(entry);
        await _journalRepo.SaveChangesAsync();

        run.Status = "Paid";
        run.PostedJournalEntryId = entry.Id;
        run.UpdatedDate = DateTime.UtcNow;
        _repo.Update(run);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, run.Id) ?? run;
        return ToDto(reloaded);
    }

    public async Task<PayRunDto> CancelAsync(int orgId, int id)
    {
        var run = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"PayRun {id} not found");

        if (run.Status != "Draft" && run.Status != "Processed")
            throw new InvalidOperationException("Only draft or processed pay runs can be cancelled.");

        run.Status = "Cancelled";
        run.UpdatedDate = DateTime.UtcNow;
        _repo.Update(run);
        await _repo.SaveChangesAsync();

        return ToDto(run);
    }

    private static PayRunDto ToDto(PayRun r)
    {
        var lineDtos = r.Lines.Select(l => new PayRunLineDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}" : string.Empty,
            EmployeeCode = l.Employee?.EmployeeCode ?? string.Empty,
            BasicSalary = l.BasicSalary,
            Hra = l.Hra,
            OtherAllowances = l.OtherAllowances,
            Deductions = l.Deductions,
            GrossPay = l.GrossPay,
            NetPay = l.NetPay
        }).ToList();

        return new PayRunDto
        {
            Id = r.Id,
            RunNumber = r.RunNumber,
            PeriodMonth = r.PeriodMonth,
            PeriodYear = r.PeriodYear,
            ExpenseLedgerAccountId = r.ExpenseLedgerAccountId,
            ExpenseLedgerAccountName = r.ExpenseLedgerAccount?.Name ?? string.Empty,
            DeductionsPayableLedgerAccountId = r.DeductionsPayableLedgerAccountId,
            DeductionsPayableLedgerAccountName = r.DeductionsPayableLedgerAccount?.Name ?? string.Empty,
            Status = r.Status,
            OwnerId = r.OwnerId,
            OwnerName = r.Owner?.Name ?? string.Empty,
            PostedJournalEntryId = r.PostedJournalEntryId,
            Lines = lineDtos,
            TotalGrossPay = lineDtos.Sum(l => l.GrossPay),
            TotalDeductions = lineDtos.Sum(l => l.Deductions),
            TotalNetPay = lineDtos.Sum(l => l.NetPay)
        };
    }
}
