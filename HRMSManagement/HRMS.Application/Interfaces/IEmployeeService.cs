using HRMS.Application.Common;
using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeListDto>> GetPagedAsync(
        int orgId, int page, int pageSize, string? search, int? departmentId, int? branchId, string? status);

    Task<List<EmployeeLookupDto>> GetLookupAsync(int orgId);

    Task<EmployeeDetailDto> GetByIdAsync(int orgId, int id);

    Task<EmployeeDetailDto> GetMyProfileAsync(int orgId, int employeeId);

    Task<EmployeeDetailDto> CreateAsync(int orgId, CreateEmployeeDto dto);

    Task<EmployeeDetailDto> UpdateAsync(int orgId, int id, UpdateEmployeeDto dto);

    Task DeleteAsync(int orgId, int id);

    Task<EmployeeDocumentDto> AddDocumentAsync(int orgId, int employeeId, CreateEmployeeDocumentDto dto);
    Task RemoveDocumentAsync(int orgId, int employeeId, int docId);

    Task<EmployeeEducationDto> AddEducationAsync(int orgId, int employeeId, CreateEmployeeEducationDto dto);
    Task RemoveEducationAsync(int orgId, int employeeId, int eduId);

    Task<EmployeeExperienceDto> AddExperienceAsync(int orgId, int employeeId, CreateEmployeeExperienceDto dto);
    Task RemoveExperienceAsync(int orgId, int employeeId, int expId);

    Task<EmployeeFamilyMemberDto> AddFamilyMemberAsync(int orgId, int employeeId, CreateEmployeeFamilyMemberDto dto);
    Task RemoveFamilyMemberAsync(int orgId, int employeeId, int memberId);

    Task<LifecycleEventDto> AddLifecycleEventAsync(int orgId, int employeeId, CreateLifecycleEventDto dto);
    Task<List<LifecycleEventDto>> GetLifecycleEventsAsync(int orgId, int employeeId);

    Task<List<EmployeeLookupDto>> GetTeamAsync(int orgId, int managerId);

    Task<string> ExportCsvAsync(int orgId);
}
