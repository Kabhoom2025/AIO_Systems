using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IDeliveryLoadRepository
{
    Task<List<DeliveryLoad>> GetAllByOrgAsync(int orgId);
    Task<DeliveryLoad?> GetByIdAsync(int orgId, int id);

    void Add(DeliveryLoad load);
    void Update(DeliveryLoad load);
    void Remove(DeliveryLoad load);
    Task SaveChangesAsync();
}
