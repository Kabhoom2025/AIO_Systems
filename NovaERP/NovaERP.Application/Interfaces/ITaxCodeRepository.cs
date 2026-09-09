using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ITaxCodeRepository
{
    Task<List<TaxCode>> GetAllByOrgAsync(int orgId);
    Task<TaxCode?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);
    void Add(TaxCode taxCode);
    void Update(TaxCode taxCode);
    void Remove(TaxCode taxCode);
    Task SaveChangesAsync();
}
