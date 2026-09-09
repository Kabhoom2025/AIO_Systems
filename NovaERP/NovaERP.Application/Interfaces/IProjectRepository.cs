using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllByOrgAsync(int orgId);
    Task<Project?> GetByIdAsync(int orgId, int id);
    Task<bool> CodeExistsAsync(int orgId, string code);

    /// <summary>(TotalCount, CompletedCount) per ProjectId across the whole org in one query —
    /// avoids N+1 when building the project list, each row needing its own task counts.</summary>
    Task<Dictionary<int, (int Total, int Completed)>> GetTaskCountsByOrgAsync(int orgId);
    Task<(int Total, int Completed)> GetTaskCountsAsync(int orgId, int projectId);
    Task<bool> HasTasksAsync(int orgId, int projectId);

    void Add(Project project);
    void Update(Project project);
    void Remove(Project project);
    Task SaveChangesAsync();
}
