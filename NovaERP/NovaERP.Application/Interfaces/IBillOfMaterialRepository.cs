using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IBillOfMaterialRepository
{
    Task<List<BillOfMaterial>> GetAllByOrgAsync(int orgId);
    Task<BillOfMaterial?> GetByIdAsync(int orgId, int id);
    Task<BillOfMaterial?> GetByProductIdAsync(int orgId, int productId);
    Task<bool> ExistsForProductAsync(int orgId, int productId);
    void Add(BillOfMaterial bom);
    void Update(BillOfMaterial bom);
    void Remove(BillOfMaterial bom);
    Task SaveChangesAsync();
}
