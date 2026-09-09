using FoodOrder.Application.DTOs;

namespace FoodOrder.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItemDTO>> GetAllAsync();
    Task<InventoryItemDTO> GetByIdAsync(int id);
    Task<IReadOnlyList<InventoryItemDTO>> GetLowStockAsync();
    Task<IReadOnlyList<InventoryItemDTO>> GetExpiringAsync(int withinDays);
    Task<InventoryItemDTO> CreateAsync(CreateInventoryItemRequest dto);
    Task<InventoryItemDTO> UpdateAsync(int id, UpdateInventoryItemRequest dto);
    Task<InventoryItemDTO> AdjustStockAsync(int id, StockAdjustmentRequest dto);
    Task<InventoryItemDTO> PurchaseStockAsync(int id, PurchaseStockRequest dto);
    Task<InventoryItemDTO> WasteStockAsync(int id, WasteStockRequest dto);
    Task<(InventoryItemDTO source, InventoryItemDTO target)> TransferStockAsync(int id, TransferStockRequest dto);
    Task<IReadOnlyList<StockTransactionDTO>> GetTransactionsAsync(int id);
    Task DeleteAsync(int id);
    Task<InventoryItemDTO?> GetByBarcodeAsync(string barcode);
}
