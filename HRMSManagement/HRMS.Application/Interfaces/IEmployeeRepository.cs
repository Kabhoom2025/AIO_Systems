using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<(List<Employee> Items, int Total)> GetPagedAsync(
        int orgId, string? search, int? departmentId, int? branchId, string? status, int page, int pageSize);

    Task<List<Employee>> GetLookupAsync(int orgId);

    Task<Employee?> GetByIdAsync(int orgId, int id);

    Task<List<Employee>> GetDirectReportsAsync(int orgId, int managerId);

    Task<List<Employee>> GetAllForExportAsync(int orgId);

    Task<int> GetNextCodeNumberAsync(int orgId);

    Task<bool> WorkEmailExistsAsync(int orgId, string workEmail, int? excludeId);

    void Add(Employee employee);
    void Update(Employee employee);

    // Documents
    Task<EmployeeDocument?> GetDocumentAsync(int employeeId, int docId);
    void AddDocument(EmployeeDocument document);
    void RemoveDocument(EmployeeDocument document);

    // Educations
    Task<EmployeeEducation?> GetEducationAsync(int employeeId, int eduId);
    void AddEducation(EmployeeEducation education);
    void RemoveEducation(EmployeeEducation education);

    // Experiences
    Task<EmployeeExperience?> GetExperienceAsync(int employeeId, int expId);
    void AddExperience(EmployeeExperience experience);
    void RemoveExperience(EmployeeExperience experience);

    // Family members
    Task<EmployeeFamilyMember?> GetFamilyMemberAsync(int employeeId, int memberId);
    void AddFamilyMember(EmployeeFamilyMember member);
    void RemoveFamilyMember(EmployeeFamilyMember member);

    // Lifecycle events
    Task<List<EmployeeLifecycleEvent>> GetLifecycleEventsAsync(int employeeId);
    void AddLifecycleEvent(EmployeeLifecycleEvent lifecycleEvent);

    Task SaveChangesAsync();
}
