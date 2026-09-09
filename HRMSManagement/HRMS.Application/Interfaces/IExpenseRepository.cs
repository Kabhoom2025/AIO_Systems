using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IExpenseRepository
{
    Task<List<ExpenseClaim>> GetAllByOrgAsync(int orgId, string? status);
    Task<List<ExpenseClaim>> GetByEmployeeAsync(int orgId, int employeeId);
    Task<ExpenseClaim?> GetByIdAsync(int id);
    void Add(ExpenseClaim claim);
    void Update(ExpenseClaim claim);
    void Remove(ExpenseClaim claim);
    Task SaveChangesAsync();
}
