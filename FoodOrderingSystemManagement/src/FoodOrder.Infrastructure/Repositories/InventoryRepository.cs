using FoodOrder.Application.Interfaces.Repositories;
using FoodOrder.Domain.Entities;
using FoodOrder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync() =>
        await _context.InventoryItems
            .OrderBy(i => i.Name)
            .ToListAsync();

    public async Task<InventoryItem?> GetByIdAsync(int id) =>
        await _context.InventoryItems.FindAsync(id);

    public async Task<IReadOnlyList<InventoryItem>> GetLowStockAsync() =>
        await _context.InventoryItems
            .Where(i => i.IsActive && i.CurrentStock <= i.MinimumStock)
            .OrderBy(i => i.CurrentStock)
            .ToListAsync();

    public async Task<IReadOnlyList<InventoryItem>> GetExpiringAsync(int withinDays)
    {
        var cutoff = DateTime.UtcNow.AddDays(withinDays);
        return await _context.InventoryItems
            .Where(i => i.IsActive && i.ExpiryDate.HasValue && i.ExpiryDate <= cutoff)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync();
    }

    public async Task AddAsync(InventoryItem item) =>
        await _context.InventoryItems.AddAsync(item);

    public async Task UpdateAsync(InventoryItem item) =>
        _context.InventoryItems.Update(item);

    public async Task DeleteAsync(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item != null) _context.InventoryItems.Remove(item);
    }

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public async Task<InventoryItem?> GetByBarcodeAsync(string barcode) =>
        await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.Barcode == barcode && i.IsActive);

    public async Task<IReadOnlyList<StockTransaction>> GetTransactionsAsync(int inventoryItemId) =>
        await _context.StockTransactions
            .Where(t => t.InventoryItemId == inventoryItemId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task AddTransactionAsync(StockTransaction transaction) =>
        await _context.StockTransactions.AddAsync(transaction);
}
