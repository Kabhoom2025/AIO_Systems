using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ILedgerRepository
{
    Task<IReadOnlyList<LedgerEntry>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<LedgerEntry?> GetByIdAsync(int id);
    Task AddAsync(LedgerEntry entry);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}
