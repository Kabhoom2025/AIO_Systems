using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repo;

    public ExpenseService(IExpenseRepository repo) => _repo = repo;

    public async Task<List<ExpenseDto>> GetAllAsync(int orgId)
    {
        var expenses = await _repo.GetAllByOrgAsync(orgId);
        return expenses.Select(MapToDto).ToList();
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var expense = await _repo.GetByIdAsync(id);
        return expense == null ? null : MapToDto(expense);
    }

    public async Task<ExpenseDto> CreateAsync(int orgId, CreateExpenseDto dto)
    {
        var expense = new Expense
        {
            OrganizationId = orgId,
            BranchId       = dto.BranchId,
            Category       = dto.Category,
            Amount         = dto.Amount,
            ExpenseDate    = DateTime.SpecifyKind(dto.ExpenseDate, DateTimeKind.Utc),
            PaymentMethod  = dto.PaymentMethod,
            Notes          = dto.Notes
        };
        _repo.Add(expense);
        await _repo.SaveChangesAsync();
        return MapToDto(expense);
    }

    public async Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseDto dto)
    {
        var expense = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Expense {id} not found");

        expense.BranchId      = dto.BranchId;
        expense.Category      = dto.Category;
        expense.Amount        = dto.Amount;
        expense.ExpenseDate   = DateTime.SpecifyKind(dto.ExpenseDate, DateTimeKind.Utc);
        expense.PaymentMethod = dto.PaymentMethod;
        expense.Notes         = dto.Notes;
        expense.UpdatedDate   = DateTime.UtcNow;

        _repo.Update(expense);
        await _repo.SaveChangesAsync();
        return MapToDto(expense);
    }

    public async Task DeleteAsync(int id)
    {
        var expense = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Expense {id} not found");
        _repo.Remove(expense);
        await _repo.SaveChangesAsync();
    }

    private static ExpenseDto MapToDto(Expense e) => new()
    {
        Id            = e.Id,
        BranchId      = e.BranchId,
        BranchName    = e.Branch?.Name ?? string.Empty,
        Category      = e.Category,
        Amount        = e.Amount,
        ExpenseDate   = e.ExpenseDate,
        PaymentMethod = e.PaymentMethod,
        Notes         = e.Notes
    };
}
