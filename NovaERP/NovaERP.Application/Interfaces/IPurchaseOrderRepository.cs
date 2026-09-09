using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrder>> GetAllByOrgAsync(int orgId);
    Task<PurchaseOrder?> GetByIdAsync(int orgId, int id);
    Task<List<PurchaseOrder>> GetByVendorIdAsync(int orgId, int vendorId);
    void Add(PurchaseOrder order);
    void Update(PurchaseOrder order);
    void Remove(PurchaseOrder order);
    Task SaveChangesAsync();
}
