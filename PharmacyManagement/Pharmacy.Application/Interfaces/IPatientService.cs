using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IPatientService
{
    Task<List<PatientDto>> GetAllAsync(int orgId);
    Task<PatientDto?> GetByIdAsync(int id);
    Task<PatientDto> CreateAsync(int orgId, CreatePatientDto dto);
    Task<PatientDto> UpdateAsync(int id, UpdatePatientDto dto);
    Task<PatientDto> UpdateProfileAsync(int patientId, UpdatePatientProfileDto dto);
    Task DeleteAsync(int id);
}
