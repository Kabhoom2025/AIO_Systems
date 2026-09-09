using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IPrescriptionService
{
    Task<List<PrescriptionDto>> GetAllAsync(int orgId);
    Task<PrescriptionDto?> GetByIdAsync(int id);
    Task<PrescriptionDto> CreateAsync(int orgId, CreatePrescriptionDto dto);
    Task<PrescriptionDto> UpdateStatusAsync(int id, string status);
    Task MarkItemDispensedAsync(int prescriptionId, int itemId);
}
