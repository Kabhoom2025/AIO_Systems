using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly AppDbContext _context;

    public LedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LedgerEntry>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);
        return await _context.LedgerEntries
            .Include(e => e.CreatedBy)
            .Where(e => e.Date >= utcFrom && e.Date <= utcTo)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<LedgerEntry?> GetByIdAsync(int id) =>
        await _context.LedgerEntries.FindAsync(id);

    public async Task AddAsync(LedgerEntry entry) =>
        await _context.LedgerEntries.AddAsync(entry);

    public async Task DeleteAsync(int id)
    {
        var entry = await _context.LedgerEntries.FindAsync(id);
        if (entry != null) _context.LedgerEntries.Remove(entry);
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
