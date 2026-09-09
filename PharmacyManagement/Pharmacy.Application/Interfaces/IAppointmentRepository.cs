using Pharmacy.Domain.Entities;

namespace Pharmacy.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetByPatientAsync(int patientId);
    Task<List<Appointment>> GetAllByOrgAsync(int orgId);
    Task<Appointment?> GetByIdAsync(int id);
    void Add(Appointment appointment);
    Task SaveChangesAsync();
}
