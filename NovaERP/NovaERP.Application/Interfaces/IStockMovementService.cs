using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IStockMovementService
{
    Task<List<StockMovementDto>> GetAllAsync(int orgId, int? productId);
    Task<StockMovementDto> CreateAsync(int orgId, CreateStockMovementDto dto);
    Task<decimal> GetOnHandAtWarehouseAsync(int orgId, int productId, int warehouseId);
}
