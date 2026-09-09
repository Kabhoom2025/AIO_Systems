using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(int id);
    Task<List<int>> GetAllActiveIdsAsync();
    void Update(Organization organization);
    Task SaveChangesAsync();
}
