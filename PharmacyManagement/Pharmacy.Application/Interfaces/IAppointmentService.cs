using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(int patientId, int orgId, CreateAppointmentDto dto);
    Task<List<AppointmentDto>> GetByPatientAsync(int patientId);
    Task CancelAsync(int patientId, int appointmentId);
    Task<List<AppointmentDto>> GetAllAsync(int orgId);
    Task<AppointmentDto> UpdateStatusAsync(int appointmentId, string status);
}
