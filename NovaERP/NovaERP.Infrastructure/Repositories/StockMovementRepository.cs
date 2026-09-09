using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly NovaErpDbContext _ctx;

    public StockMovementRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<StockMovement>> GetAllByOrgAsync(int orgId, int? productId)
    {
        var query = _ctx.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Where(m => m.OrganizationId == orgId);

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        return query.OrderByDescending(m => m.MovementDate).ToListAsync();
    }

    public Task<decimal> GetOnHandAtWarehouseAsync(int orgId, int productId, int warehouseId) =>
        _ctx.StockMovements
            .Where(m => m.OrganizationId == orgId && m.ProductId == productId && m.WarehouseId == warehouseId)
            .SumAsync(m => m.Quantity);

    public void Add(StockMovement movement) => _ctx.StockMovements.Add(movement);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
