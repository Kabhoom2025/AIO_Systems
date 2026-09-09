using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ISalesOrderRepository
{
    Task<List<SalesOrder>> GetAllByOrgAsync(int orgId);
    Task<SalesOrder?> GetByIdAsync(int orgId, int id);
    Task<List<SalesOrder>> GetByAccountIdAsync(int orgId, int accountId);
    Task<List<SalesOrder>> GetByOwnerIdAsync(int orgId, int ownerId);
    void Add(SalesOrder order);
    void Update(SalesOrder order);
    void Remove(SalesOrder order);
    Task SaveChangesAsync();
}
