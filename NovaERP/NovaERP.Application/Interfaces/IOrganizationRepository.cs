using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(int id);
    void Add(Organization organization);
    void Update(Organization organization);
    void Remove(Organization organization);
    Task SaveChangesAsync();
}
