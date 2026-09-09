using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class TableReservationRepository : ITableReservationRepository
{
    private readonly AppDbContext _context;

    public TableReservationRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<TableReservation>> GetAllAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.TableReservations.Include(r => r.Table).AsQueryable();

        if (from.HasValue) query = query.Where(r => r.ReservationDateTime >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.ReservationDateTime <= to.Value);

        return await query.OrderBy(r => r.ReservationDateTime).ToListAsync();
    }

    public async Task<IReadOnlyList<TableReservation>> GetByTableAsync(int tableId) =>
        await _context.TableReservations
            .Include(r => r.Table)
            .Where(r => r.TableId == tableId)
            .OrderBy(r => r.ReservationDateTime)
            .ToListAsync();

    public async Task<IReadOnlyList<TableReservation>> GetUpcomingAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.TableReservations
            .Include(r => r.Table)
            .Where(r => r.ReservationDateTime >= now
                     && r.Status != ReservationStatus.Cancelled
                     && r.Status != ReservationStatus.NoShow)
            .OrderBy(r => r.ReservationDateTime)
            .ToListAsync();
    }

    public async Task<TableReservation?> GetByIdAsync(int id) =>
        await _context.TableReservations
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(TableReservation reservation) =>
        await _context.TableReservations.AddAsync(reservation);

    public Task UpdateAsync(TableReservation reservation)
    {
        _context.TableReservations.Update(reservation);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var r = await _context.TableReservations.FindAsync(id);
        if (r is not null) _context.TableReservations.Remove(r);
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
