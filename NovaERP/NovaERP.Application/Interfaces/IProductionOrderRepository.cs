using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IProductionOrderRepository
{
    Task<List<ProductionOrder>> GetAllByOrgAsync(int orgId);
    Task<ProductionOrder?> GetByIdAsync(int orgId, int id);
    void Add(ProductionOrder order);
    void Update(ProductionOrder order);
    void Remove(ProductionOrder order);
    Task SaveChangesAsync();
}
