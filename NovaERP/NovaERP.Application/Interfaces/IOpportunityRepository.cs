using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IOpportunityRepository
{
    Task<List<Opportunity>> GetAllByOrgAsync(int orgId);
    Task<Opportunity?> GetByIdAsync(int orgId, int id);
    void Add(Opportunity opportunity);
    void Update(Opportunity opportunity);
    void Remove(Opportunity opportunity);
    Task SaveChangesAsync();
}
