using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IMedicineService
{
    Task<List<MedicineDto>> GetAllAsync(int orgId);
    Task<List<PublicMedicineDto>> GetPublicListAsync(int orgId);
    Task<MedicineDto?> GetByIdAsync(int id);
    Task<MedicineDto?> GetByCodeAsync(int orgId, string code);
    Task<MedicineDto> CreateAsync(int orgId, CreateMedicineDto dto);
    Task<MedicineDto> UpdateAsync(int id, UpdateMedicineDto dto);
    Task DeleteAsync(int id);
    Task<MedicineBatchDto> AddBatchAsync(int medicineId, AddBatchDto dto);
    Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync(int orgId, int daysThreshold = 90);
}
