using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IDoctorRepository
{
    Task<List<Doctor>> GetAllByOrgAsync(int orgId);
    Task<Doctor?> GetByIdAsync(int id);
    void Add(Doctor doctor);
    void Update(Doctor doctor);
    void Remove(Doctor doctor);
    Task SaveChangesAsync();
}
