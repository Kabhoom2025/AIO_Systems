using FoodOrder.Domain.Entities;

namespace FoodOrder.Application.Interfaces.Repositories;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<IReadOnlyList<InventoryItem>> GetLowStockAsync();
    Task<IReadOnlyList<InventoryItem>> GetExpiringAsync(int withinDays);
    Task AddAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
    Task<InventoryItem?> GetByBarcodeAsync(string barcode);
    Task<IReadOnlyList<StockTransaction>> GetTransactionsAsync(int inventoryItemId);
    Task AddTransactionAsync(StockTransaction transaction);
}
