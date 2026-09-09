using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IBranchRepository
{
    Task<List<Branch>> GetAllByOrgAsync(int orgId);
    Task<Branch?> GetByIdAsync(int orgId, int id);
    void Add(Branch branch);
    void Update(Branch branch);
    void Remove(Branch branch);
    Task SaveChangesAsync();
}
