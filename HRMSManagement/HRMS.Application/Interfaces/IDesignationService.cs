using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IDesignationService
{
    Task<List<DesignationDto>> GetAllAsync(int orgId);
    Task<DesignationDto> GetByIdAsync(int orgId, int id);
    Task<DesignationDto> CreateAsync(int orgId, CreateDesignationDto dto);
    Task<DesignationDto> UpdateAsync(int orgId, int id, UpdateDesignationDto dto);
    Task DeleteAsync(int orgId, int id);

    Task<List<JobGradeDto>> GetJobGradesAsync(int orgId);
    Task<JobGradeDto> CreateJobGradeAsync(int orgId, CreateJobGradeDto dto);
    Task<JobGradeDto> UpdateJobGradeAsync(int orgId, int id, UpdateJobGradeDto dto);
    Task DeleteJobGradeAsync(int orgId, int id);
}
