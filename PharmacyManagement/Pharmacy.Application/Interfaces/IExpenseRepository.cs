using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllByOrgAsync(int orgId);
    Task<Expense?> GetByIdAsync(int id);
    void Add(Expense expense);
    void Update(Expense expense);
    void Remove(Expense expense);
    Task SaveChangesAsync();
}
