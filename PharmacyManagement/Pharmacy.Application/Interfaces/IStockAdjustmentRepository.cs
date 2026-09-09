using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IStockAdjustmentRepository
{
    Task<List<StockAdjustment>> GetAllByOrgAsync(int orgId);
    Task<StockAdjustment?> GetByIdAsync(int id);
    Task<MedicineBatch?> GetBatchAsync(int batchId);
    void Add(StockAdjustment adjustment);
    Task SaveChangesAsync();
}
