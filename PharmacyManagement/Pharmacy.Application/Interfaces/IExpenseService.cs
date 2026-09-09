using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetAllAsync(int orgId);
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> CreateAsync(int orgId, CreateExpenseDto dto);
    Task<ExpenseDto> UpdateAsync(int id, UpdateExpenseDto dto);
    Task DeleteAsync(int id);
}
