using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class GoodsReceiptRepository : IGoodsReceiptRepository
{
    private readonly PharmacyDbContext _ctx;

    public GoodsReceiptRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<GoodsReceipt>> GetAllByOrgAsync(int orgId) =>
        _ctx.GoodsReceipts
            .Include(g => g.PurchaseOrder)
            .Include(g => g.Items).ThenInclude(i => i.Medicine)
            .Where(g => g.OrganizationId == orgId)
            .OrderByDescending(g => g.ReceivedDate)
            .ToListAsync();

    public Task<GoodsReceipt?> GetByIdAsync(int id) =>
        _ctx.GoodsReceipts
            .Include(g => g.PurchaseOrder)
            .Include(g => g.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(g => g.Id == id);

    public Task<PurchaseOrder?> GetPurchaseOrderWithItemsAsync(int purchaseOrderId) =>
        _ctx.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

    public void Add(GoodsReceipt goodsReceipt) => _ctx.GoodsReceipts.Add(goodsReceipt);
    public void AddBatch(MedicineBatch batch)  => _ctx.MedicineBatches.Add(batch);
    public Task SaveChangesAsync()             => _ctx.SaveChangesAsync();
}
