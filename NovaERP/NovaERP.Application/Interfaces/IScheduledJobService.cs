using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IScheduledJobService
{
    Task<List<ScheduledJobDefinitionDto>> GetAllAsync(int orgId);
    Task<ScheduledJobDefinitionDto> UpdateAsync(int orgId, int id, UpdateScheduledJobDto dto);
    Task<ScheduledJobDefinitionDto> GetByIdAsync(int orgId, int id);
}
