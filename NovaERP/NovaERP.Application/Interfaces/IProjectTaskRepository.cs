using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IProjectTaskRepository
{
    Task<List<ProjectTask>> GetAllByOrgAsync(int orgId);
    Task<ProjectTask?> GetByIdAsync(int orgId, int id);

    void Add(ProjectTask task);
    void Update(ProjectTask task);
    void Remove(ProjectTask task);
    Task SaveChangesAsync();
}
