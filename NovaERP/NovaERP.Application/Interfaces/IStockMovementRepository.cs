using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IStockMovementRepository
{
    Task<List<StockMovement>> GetAllByOrgAsync(int orgId, int? productId);
    Task<decimal> GetOnHandAtWarehouseAsync(int orgId, int productId, int warehouseId);
    void Add(StockMovement movement);
    Task SaveChangesAsync();
}
