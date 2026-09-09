using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IPrescriptionRepository
{
    Task<List<Prescription>> GetAllByOrgAsync(int orgId);
    Task<Prescription?> GetByIdAsync(int id);
    void Add(Prescription prescription);
    void Update(Prescription prescription);
    Task SaveChangesAsync();
}
