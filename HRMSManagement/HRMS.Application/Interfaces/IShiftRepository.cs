using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces;

public interface IShiftRepository
{
    Task<List<Shift>> GetAllByOrgAsync(int orgId);
    Task<Shift?> GetByIdAsync(int orgId, int id);
    void Add(Shift shift);
    void Update(Shift shift);
    void Remove(Shift shift);
    Task SaveChangesAsync();
}
