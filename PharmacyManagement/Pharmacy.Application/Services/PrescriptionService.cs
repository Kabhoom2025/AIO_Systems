using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _repo;

    public PrescriptionService(IPrescriptionRepository repo) => _repo = repo;

    public async Task<List<PrescriptionDto>> GetAllAsync(int orgId)
    {
        var list = await _repo.GetAllByOrgAsync(orgId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PrescriptionDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p == null ? null : MapToDto(p);
    }

    public async Task<PrescriptionDto> CreateAsync(int orgId, CreatePrescriptionDto dto)
    {
        var prescription = new Prescription
        {
            OrganizationId   = orgId,
            PatientName      = dto.PatientName,
            PatientPhone     = dto.PatientPhone,
            PatientAge       = dto.PatientAge,
            DoctorName       = dto.DoctorName,
            DoctorRegNo      = dto.DoctorRegNo,
            HospitalName     = dto.HospitalName,
            PrescriptionDate = dto.PrescriptionDate,
            ImageBase64      = dto.ImageBase64,
            Notes            = dto.Notes,
            Status           = "Pending",
            Items = dto.Items.Select(i => new PrescriptionItem
            {
                MedicineName = i.MedicineName,
                Dosage       = i.Dosage,
                Duration     = i.Duration,
                Quantity     = i.Quantity
            }).ToList()
        };
        _repo.Add(prescription);
        await _repo.SaveChangesAsync();
        return MapToDto(prescription);
    }

    public async Task<PrescriptionDto> UpdateStatusAsync(int id, string status)
    {
        var prescription = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Prescription {id} not found");
        prescription.Status = status;
        _repo.Update(prescription);
        await _repo.SaveChangesAsync();
        return MapToDto(prescription);
    }

    public async Task MarkItemDispensedAsync(int prescriptionId, int itemId)
    {
        var prescription = await _repo.GetByIdAsync(prescriptionId)
            ?? throw new KeyNotFoundException($"Prescription {prescriptionId} not found");

        var item = prescription.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found");

        item.IsDispensed = true;

        var allDispensed = prescription.Items.All(i => i.IsDispensed);
        var anyDispensed = prescription.Items.Any(i => i.IsDispensed);
        prescription.Status = allDispensed ? "Dispensed" : (anyDispensed ? "Partial" : "Pending");

        _repo.Update(prescription);
        await _repo.SaveChangesAsync();
    }

    private static PrescriptionDto MapToDto(Prescription p) => new()
    {
        Id               = p.Id,
        PatientName      = p.PatientName,
        PatientPhone     = p.PatientPhone,
        PatientAge       = p.PatientAge,
        DoctorName       = p.DoctorName,
        DoctorRegNo      = p.DoctorRegNo,
        HospitalName     = p.HospitalName,
        PrescriptionDate = p.PrescriptionDate,
        ImageBase64      = p.ImageBase64,
        Status           = p.Status,
        Notes            = p.Notes,
        CreatedDate      = p.CreatedDate,
        Items = p.Items.Select(i => new PrescriptionItemDto
        {
            Id           = i.Id,
            MedicineName = i.MedicineName,
            Dosage       = i.Dosage,
            Duration     = i.Duration,
            Quantity     = i.Quantity,
            IsDispensed  = i.IsDispensed
        }).ToList()
    };
}
