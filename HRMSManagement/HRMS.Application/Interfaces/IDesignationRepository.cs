using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IDesignationRepository
{
    Task<List<Designation>> GetAllByOrgAsync(int orgId);
    Task<Designation?> GetByIdAsync(int orgId, int id);
    void Add(Designation designation);
    void Update(Designation designation);
    void Remove(Designation designation);

    Task<List<JobGrade>> GetJobGradesByOrgAsync(int orgId);
    Task<JobGrade?> GetJobGradeByIdAsync(int orgId, int id);
    Task<bool> IsJobGradeInUseAsync(int jobGradeId);
    void AddJobGrade(JobGrade jobGrade);
    void UpdateJobGrade(JobGrade jobGrade);
    void RemoveJobGrade(JobGrade jobGrade);

    Task SaveChangesAsync();
}
