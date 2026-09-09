using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IPatientRepository
{
    Task<List<Patient>> GetAllByOrgAsync(int orgId);
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByPhoneAsync(int orgId, string phone);
    void Add(Patient patient);
    void Update(Patient patient);
    void Remove(Patient patient);
    Task SaveChangesAsync();
}
