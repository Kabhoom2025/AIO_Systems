using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface ILeadRepository
{
    Task<List<Lead>> GetAllByOrgAsync(int orgId);
    Task<Lead?> GetByIdAsync(int orgId, int id);
    void Add(Lead lead);
    void Update(Lead lead);
    void Remove(Lead lead);
    Task SaveChangesAsync();
}
