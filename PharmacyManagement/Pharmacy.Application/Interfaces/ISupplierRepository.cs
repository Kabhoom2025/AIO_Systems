using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetAllByOrgAsync(int orgId);
    Task<Supplier?> GetByIdAsync(int id);
    void Add(Supplier supplier);
    void Update(Supplier supplier);
    void Remove(Supplier supplier);
    Task SaveChangesAsync();
}
