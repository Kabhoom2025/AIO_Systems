using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IMedicineRepository
{
    Task<List<Medicine>> GetAllByOrgAsync(int orgId);
    Task<Medicine?> GetByIdAsync(int id);
    Task<Medicine?> GetByCodeAsync(int orgId, string code);
    Task<List<MedicineBatch>> GetExpiryAlertsAsync(int orgId, int daysThreshold);
    void Add(Medicine medicine);
    void Update(Medicine medicine);
    void Remove(Medicine medicine);
    void AddBatch(MedicineBatch batch);
    Task SaveChangesAsync();
}
