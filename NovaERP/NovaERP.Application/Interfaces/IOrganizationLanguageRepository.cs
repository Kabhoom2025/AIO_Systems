using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IOrganizationLanguageRepository
{
    Task<List<OrganizationLanguage>> GetAllByOrgAsync(int orgId);
    void AddRange(IEnumerable<OrganizationLanguage> items);
    void RemoveRange(IEnumerable<OrganizationLanguage> items);
    Task SaveChangesAsync();
}
