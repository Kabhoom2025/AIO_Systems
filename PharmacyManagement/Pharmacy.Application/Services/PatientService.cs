using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;

    public PatientService(IPatientRepository repo) => _repo = repo;

    public async Task<List<PatientDto>> GetAllAsync(int orgId)
    {
        var patients = await _repo.GetAllByOrgAsync(orgId);
        return patients.Select(MapToDto).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id);
        return patient == null ? null : MapToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(int orgId, CreatePatientDto dto)
    {
        var patient = new Patient
        {
            OrganizationId        = orgId,
            Name                  = dto.Name,
            Phone                 = dto.Phone,
            Email                 = dto.Email,
            DateOfBirth           = AsUtc(dto.DateOfBirth),
            Gender                = dto.Gender,
            Address               = dto.Address,
            BloodGroup            = dto.BloodGroup,
            Allergies             = dto.Allergies,
            ChronicDiseases       = dto.ChronicDiseases,
            EmergencyContactName  = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            InsuranceProvider     = dto.InsuranceProvider,
            InsurancePolicyNumber = dto.InsurancePolicyNumber,
            IsActive              = true
        };
        _repo.Add(patient);
        await _repo.SaveChangesAsync();
        return MapToDto(patient);
    }

    public async Task<PatientDto> UpdateAsync(int id, UpdatePatientDto dto)
    {
        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient {id} not found");

        patient.Name                  = dto.Name;
        patient.Phone                 = dto.Phone;
        patient.Email                 = dto.Email;
        patient.DateOfBirth           = AsUtc(dto.DateOfBirth);
        patient.Gender                = dto.Gender;
        patient.Address               = dto.Address;
        patient.BloodGroup            = dto.BloodGroup;
        patient.Allergies             = dto.Allergies;
        patient.ChronicDiseases       = dto.ChronicDiseases;
        patient.EmergencyContactName  = dto.EmergencyContactName;
        patient.EmergencyContactPhone = dto.EmergencyContactPhone;
        patient.InsuranceProvider     = dto.InsuranceProvider;
        patient.InsurancePolicyNumber = dto.InsurancePolicyNumber;
        patient.IsActive              = dto.IsActive;
        patient.UpdatedDate           = DateTime.UtcNow;

        _repo.Update(patient);
        await _repo.SaveChangesAsync();
        return MapToDto(patient);
    }

    public async Task<PatientDto> UpdateProfileAsync(int patientId, UpdatePatientProfileDto dto)
    {
        var patient = await _repo.GetByIdAsync(patientId)
            ?? throw new KeyNotFoundException($"Patient {patientId} not found");

        patient.Name                  = dto.Name;
        patient.Email                 = dto.Email;
        patient.DateOfBirth           = AsUtc(dto.DateOfBirth);
        patient.Gender                = dto.Gender;
        patient.Address               = dto.Address;
        patient.BloodGroup            = dto.BloodGroup;
        patient.Allergies             = dto.Allergies;
        patient.ChronicDiseases       = dto.ChronicDiseases;
        patient.EmergencyContactName  = dto.EmergencyContactName;
        patient.EmergencyContactPhone = dto.EmergencyContactPhone;
        patient.InsuranceProvider     = dto.InsuranceProvider;
        patient.InsurancePolicyNumber = dto.InsurancePolicyNumber;
        patient.UpdatedDate           = DateTime.UtcNow;

        _repo.Update(patient);
        await _repo.SaveChangesAsync();
        return MapToDto(patient);
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Patient {id} not found");
        _repo.Remove(patient);
        await _repo.SaveChangesAsync();
    }

    private static DateTime? AsUtc(DateTime? dt) =>
        dt.HasValue ? DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc) : null;

    private static PatientDto MapToDto(Patient p) => new()
    {
        Id                    = p.Id,
        Name                  = p.Name,
        Phone                 = p.Phone,
        Email                 = p.Email,
        DateOfBirth           = p.DateOfBirth,
        Gender                = p.Gender,
        Address               = p.Address,
        BloodGroup            = p.BloodGroup,
        Allergies             = p.Allergies,
        ChronicDiseases       = p.ChronicDiseases,
        EmergencyContactName  = p.EmergencyContactName,
        EmergencyContactPhone = p.EmergencyContactPhone,
        InsuranceProvider     = p.InsuranceProvider,
        InsurancePolicyNumber = p.InsurancePolicyNumber,
        IsActive              = p.IsActive,
        IsVerified            = p.IsVerified
    };
}
