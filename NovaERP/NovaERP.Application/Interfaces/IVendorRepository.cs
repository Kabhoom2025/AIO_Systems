using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IVendorRepository
{
    Task<List<Vendor>> GetAllByOrgAsync(int orgId);
    Task<Vendor?> GetByIdAsync(int orgId, int id);
    Task<Vendor?> GetByUserIdAsync(int orgId, int userId);
    void Add(Vendor vendor);
    void Update(Vendor vendor);
    void Remove(Vendor vendor);
    Task SaveChangesAsync();
}
