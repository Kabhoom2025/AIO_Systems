using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class StockAdjustmentRepository : IStockAdjustmentRepository
{
    private readonly PharmacyDbContext _ctx;

    public StockAdjustmentRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<StockAdjustment>> GetAllByOrgAsync(int orgId) =>
        _ctx.StockAdjustments
            .Include(a => a.Medicine)
            .Include(a => a.MedicineBatch)
            .Where(a => a.OrganizationId == orgId)
            .OrderByDescending(a => a.AdjustedDate)
            .ToListAsync();

    public Task<StockAdjustment?> GetByIdAsync(int id) =>
        _ctx.StockAdjustments
            .Include(a => a.Medicine)
            .Include(a => a.MedicineBatch)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<MedicineBatch?> GetBatchAsync(int batchId) =>
        _ctx.MedicineBatches.FirstOrDefaultAsync(b => b.Id == batchId);

    public void Add(StockAdjustment adjustment) => _ctx.StockAdjustments.Add(adjustment);
    public Task SaveChangesAsync()              => _ctx.SaveChangesAsync();
}
