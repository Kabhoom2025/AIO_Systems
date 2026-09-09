using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface ITableReservationRepository
{
    Task<IReadOnlyList<TableReservation>> GetAllAsync(DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<TableReservation>> GetByTableAsync(int tableId);
    Task<IReadOnlyList<TableReservation>> GetUpcomingAsync();
    Task<TableReservation?> GetByIdAsync(int id);
    Task AddAsync(TableReservation reservation);
    Task UpdateAsync(TableReservation reservation);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}
