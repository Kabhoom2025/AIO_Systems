using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IShippingConnectorRepository
{
    Task<List<ShippingConnector>> GetAllByOrgAsync(int orgId);
    Task<List<ShippingConnector>> GetActiveByOrgAsync(int orgId);
    Task<ShippingConnector?> GetByIdAsync(int orgId, int id);

    void Add(ShippingConnector connector);
    void Update(ShippingConnector connector);
    void Remove(ShippingConnector connector);
    Task SaveChangesAsync();
}
