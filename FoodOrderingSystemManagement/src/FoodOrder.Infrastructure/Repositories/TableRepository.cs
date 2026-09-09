using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class TableRepository : GenericRepository<Table>, ITableRepository
{
    public TableRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Table>> GetAllWithStatusAsync()
    {
        return await _context.Tables
            .Include(t => t.Orders)
            .OrderBy(t => t.Hall)
            .ThenBy(t => t.TableNumber)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNumberAsync(int tableNumber, string hall, int? excludeId = null)
    {
        return await _context.Tables.AnyAsync(t =>
            t.TableNumber == tableNumber &&
            t.Hall == hall &&
            (excludeId == null || t.Id != excludeId));
    }

    // Anonymous public order placement has no JWT/tenant claims — bypass the
    // ambient filter and check the branch explicitly instead.
    public async Task<Table?> GetByIdForBranchAsync(int id, int branchId) =>
        await _context.Tables
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && t.BranchId == branchId);
}
