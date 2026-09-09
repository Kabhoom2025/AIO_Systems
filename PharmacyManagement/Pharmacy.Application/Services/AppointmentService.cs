using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Services;

public class AppointmentService : IAppointmentService
{
    private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase) { "Completed", "Cancelled" };
    private static readonly HashSet<string> AllStatuses = new(StringComparer.OrdinalIgnoreCase) { "Requested", "Confirmed", "Completed", "Cancelled" };

    private readonly IAppointmentRepository _repo;

    public AppointmentService(IAppointmentRepository repo) => _repo = repo;

    public async Task<AppointmentDto> CreateAsync(int patientId, int orgId, CreateAppointmentDto dto)
    {
        var appointment = new Appointment
        {
            OrganizationId = orgId,
            PatientId      = patientId,
            DoctorId       = dto.DoctorId,
            BranchId       = dto.BranchId,
            PreferredDate  = DateTime.SpecifyKind(dto.PreferredDate.Date, DateTimeKind.Utc),
            TimeSlot       = dto.TimeSlot,
            Notes          = dto.Notes,
            Status         = "Requested"
        };
        _repo.Add(appointment);
        await _repo.SaveChangesAsync();

        var saved = await _repo.GetByIdAsync(appointment.Id)
            ?? throw new InvalidOperationException("Appointment was not saved correctly.");
        return MapToDto(saved);
    }

    public async Task<List<AppointmentDto>> GetByPatientAsync(int patientId)
    {
        var appointments = await _repo.GetByPatientAsync(patientId);
        return appointments.Select(MapToDto).ToList();
    }

    public async Task CancelAsync(int patientId, int appointmentId)
    {
        var appointment = await _repo.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException($"Appointment {appointmentId} not found");

        if (appointment.PatientId != patientId)
            throw new UnauthorizedAccessException("This appointment does not belong to you.");

        if (TerminalStatuses.Contains(appointment.Status))
            throw new InvalidOperationException($"An appointment that is already {appointment.Status} cannot be cancelled.");

        appointment.Status = "Cancelled";
        appointment.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
    }

    public async Task<List<AppointmentDto>> GetAllAsync(int orgId)
    {
        var appointments = await _repo.GetAllByOrgAsync(orgId);
        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentDto> UpdateStatusAsync(int appointmentId, string status)
    {
        if (!AllStatuses.Contains(status))
            throw new InvalidOperationException($"Unknown appointment status '{status}'.");

        var appointment = await _repo.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException($"Appointment {appointmentId} not found");

        if (TerminalStatuses.Contains(appointment.Status))
            throw new InvalidOperationException($"An appointment that is already {appointment.Status} cannot be changed.");

        appointment.Status = status;
        appointment.UpdatedDate = DateTime.UtcNow;
        await _repo.SaveChangesAsync();
        return MapToDto(appointment);
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id              = a.Id,
        PatientId       = a.PatientId,
        PatientName     = a.Patient?.Name ?? string.Empty,
        PatientPhone    = a.Patient?.Phone,
        DoctorId        = a.DoctorId,
        DoctorName      = a.Doctor.Name,
        DoctorSpecialty = a.Doctor.Specialty,
        BranchId        = a.BranchId,
        BranchName      = a.Branch.Name,
        PreferredDate   = a.PreferredDate,
        TimeSlot        = a.TimeSlot,
        Status          = a.Status,
        Notes           = a.Notes,
        CreatedDate     = a.CreatedDate
    };
}
