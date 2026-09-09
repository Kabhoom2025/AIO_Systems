using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IScheduledJobRepository
{
    /// <summary>Org-scoped rows union platform-wide (OrganizationId == null) rows.</summary>
    Task<List<ScheduledJobDefinition>> GetVisibleToOrgAsync(int orgId);
    Task<ScheduledJobDefinition?> GetByIdAsync(int orgId, int id);
    Task<List<ScheduledJobDefinition>> GetAllEnabledAsync();
    void Update(ScheduledJobDefinition job);
    Task SaveChangesAsync();
}
