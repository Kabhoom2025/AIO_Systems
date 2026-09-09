using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly PharmacyDbContext _ctx;

    public AppointmentRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Appointment>> GetByPatientAsync(int patientId) =>
        _ctx.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Branch)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.PreferredDate)
            .ToListAsync();

    public Task<List<Appointment>> GetAllByOrgAsync(int orgId) =>
        _ctx.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Branch)
            .Include(a => a.Patient)
            .Where(a => a.OrganizationId == orgId)
            .OrderByDescending(a => a.PreferredDate)
            .ToListAsync();

    public Task<Appointment?> GetByIdAsync(int id) =>
        _ctx.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Branch)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id);

    public void Add(Appointment appointment) => _ctx.Appointments.Add(appointment);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
