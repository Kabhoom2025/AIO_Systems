using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repo;

    public DoctorService(IDoctorRepository repo) => _repo = repo;

    public async Task<List<DoctorDto>> GetAllAsync(int orgId)
    {
        var doctors = await _repo.GetAllByOrgAsync(orgId);
        return doctors.Select(MapToDto).ToList();
    }

    public async Task<List<PublicDoctorDto>> GetPublicListAsync(int orgId)
    {
        var doctors = await _repo.GetAllByOrgAsync(orgId);
        return doctors
            .Where(d => d.IsActive)
            .Select(d => new PublicDoctorDto
            {
                Id           = d.Id,
                Name         = d.Name,
                Specialty    = d.Specialty,
                HospitalName = d.HospitalName
            })
            .ToList();
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        var doctor = await _repo.GetByIdAsync(id);
        return doctor == null ? null : MapToDto(doctor);
    }

    public async Task<DoctorDto> CreateAsync(int orgId, CreateDoctorDto dto)
    {
        var doctor = new Doctor
        {
            OrganizationId     = orgId,
            Name               = dto.Name,
            RegistrationNumber = dto.RegistrationNumber,
            Specialty          = dto.Specialty,
            Phone              = dto.Phone,
            Email              = dto.Email,
            HospitalName       = dto.HospitalName,
            IsActive           = true
        };
        _repo.Add(doctor);
        await _repo.SaveChangesAsync();
        return MapToDto(doctor);
    }

    public async Task<DoctorDto> UpdateAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Doctor {id} not found");

        doctor.Name               = dto.Name;
        doctor.RegistrationNumber = dto.RegistrationNumber;
        doctor.Specialty          = dto.Specialty;
        doctor.Phone              = dto.Phone;
        doctor.Email              = dto.Email;
        doctor.HospitalName       = dto.HospitalName;
        doctor.IsActive           = dto.IsActive;
        doctor.UpdatedDate        = DateTime.UtcNow;

        _repo.Update(doctor);
        await _repo.SaveChangesAsync();
        return MapToDto(doctor);
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Doctor {id} not found");
        _repo.Remove(doctor);
        await _repo.SaveChangesAsync();
    }

    private static DoctorDto MapToDto(Doctor d) => new()
    {
        Id                 = d.Id,
        Name               = d.Name,
        RegistrationNumber = d.RegistrationNumber,
        Specialty          = d.Specialty,
        Phone              = d.Phone,
        Email              = d.Email,
        HospitalName       = d.HospitalName,
        IsActive           = d.IsActive
    };
}
