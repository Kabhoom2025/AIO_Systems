using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IDoctorService
{
    Task<List<DoctorDto>> GetAllAsync(int orgId);
    Task<List<PublicDoctorDto>> GetPublicListAsync(int orgId);
    Task<DoctorDto?> GetByIdAsync(int id);
    Task<DoctorDto> CreateAsync(int orgId, CreateDoctorDto dto);
    Task<DoctorDto> UpdateAsync(int id, UpdateDoctorDto dto);
    Task DeleteAsync(int id);
}
