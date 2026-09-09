using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(int orgId);
    Task<ProjectDto> GetByIdAsync(int orgId, int id);
    Task<ProjectDto> CreateAsync(int orgId, CreateProjectDto dto);
    Task<ProjectDto> UpdateAsync(int orgId, int id, UpdateProjectDto dto);
    Task DeleteAsync(int orgId, int id);
}
