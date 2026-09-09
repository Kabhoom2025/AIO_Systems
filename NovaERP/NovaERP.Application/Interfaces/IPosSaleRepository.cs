using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IPosSaleRepository
{
    Task<List<PosSale>> GetAllByOrgAsync(int orgId);
    Task<PosSale?> GetByIdAsync(int orgId, int id);

    void Add(PosSale sale);
    void Update(PosSale sale);
    void Remove(PosSale sale);
    Task SaveChangesAsync();
}
