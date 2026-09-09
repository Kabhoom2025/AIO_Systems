using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IGoodsReceiptRepository
{
    Task<List<GoodsReceipt>> GetAllByOrgAsync(int orgId);
    Task<GoodsReceipt?> GetByIdAsync(int id);
    Task<PurchaseOrder?> GetPurchaseOrderWithItemsAsync(int purchaseOrderId);
    void Add(GoodsReceipt goodsReceipt);
    void AddBatch(MedicineBatch batch);
    Task SaveChangesAsync();
}
