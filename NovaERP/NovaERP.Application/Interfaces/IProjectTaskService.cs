using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IProjectTaskService
{
    Task<List<ProjectTaskDto>> GetAllAsync(int orgId);
    Task<ProjectTaskDto> GetByIdAsync(int orgId, int id);
    Task<ProjectTaskDto> CreateAsync(int orgId, CreateProjectTaskDto dto);
    Task<ProjectTaskDto> UpdateAsync(int orgId, int id, UpdateProjectTaskDto dto);
    Task DeleteAsync(int orgId, int id);
}
