using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ISettingsRepository
{
    Task<OrganizationSettings?> GetByOrgAsync(int orgId);
    void Add(OrganizationSettings settings);
    void Update(OrganizationSettings settings);
    Task SaveChangesAsync();
}
