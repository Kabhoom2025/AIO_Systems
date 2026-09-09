using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetAllByOrgAsync(int orgId);
    Task<Vehicle?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    void Add(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Remove(Vehicle vehicle);
    Task SaveChangesAsync();
}
