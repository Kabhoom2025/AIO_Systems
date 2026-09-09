using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface ISaleRepository
{
    Task<List<Sale>> GetAllByOrgAsync(int orgId);
    Task<List<Sale>> GetByPatientAsync(int patientId);
    Task<Sale?> GetByIdAsync(int id);
    Task<Medicine?> GetMedicineWithBatchesAsync(int medicineId);
    Task<Customer?> GetCustomerAsync(int customerId);
    void Add(Sale sale);
    Task SaveChangesAsync();
}
