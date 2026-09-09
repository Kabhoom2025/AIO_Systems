using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IStockAdjustmentService
{
    Task<List<StockAdjustmentDto>> GetAllAsync(int orgId);
    Task<StockAdjustmentDto?> GetByIdAsync(int id);
    Task<StockAdjustmentDto> CreateAsync(int orgId, CreateStockAdjustmentDto dto);
}
