using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class StockTransferRepository : IStockTransferRepository
{
    private readonly NovaErpDbContext _ctx;

    public StockTransferRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<StockTransfer>> GetAllByOrgAsync(int orgId) =>
        _ctx.StockTransfers
            .Include(t => t.Product)
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Where(t => t.OrganizationId == orgId)
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync();

    public void Add(StockTransfer transfer) => _ctx.StockTransfers.Add(transfer);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
