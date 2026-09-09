using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrder>> GetAllByOrgAsync(int orgId);
    Task<PurchaseOrder?> GetByIdAsync(int id);
    void Add(PurchaseOrder purchaseOrder);
    void Remove(PurchaseOrder purchaseOrder);
    Task SaveChangesAsync();
}
