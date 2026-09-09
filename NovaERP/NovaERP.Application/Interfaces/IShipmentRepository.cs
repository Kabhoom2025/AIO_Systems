using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IShipmentRepository
{
    Task<List<Shipment>> GetAllByOrgAsync(int orgId);
    Task<Shipment?> GetByIdAsync(int orgId, int id);
    Task<List<Shipment>> GetPickedUnassignedByWarehouseAsync(int orgId, int warehouseId);
    void Add(Shipment shipment);
    void Update(Shipment shipment);
    void Remove(Shipment shipment);
    Task SaveChangesAsync();
}
