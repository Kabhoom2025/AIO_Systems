using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly PharmacyDbContext _ctx;

    public PatientRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Patient>> GetAllByOrgAsync(int orgId) =>
        _ctx.Patients
            .Where(p => p.OrganizationId == orgId)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public Task<Patient?> GetByIdAsync(int id) =>
        _ctx.Patients.FirstOrDefaultAsync(p => p.Id == id);

    public Task<Patient?> GetByPhoneAsync(int orgId, string phone) =>
        _ctx.Patients.FirstOrDefaultAsync(p => p.OrganizationId == orgId && p.Phone == phone);

    public void Add(Patient patient)    => _ctx.Patients.Add(patient);
    public void Update(Patient patient) => _ctx.Patients.Update(patient);
    public void Remove(Patient patient) => _ctx.Patients.Remove(patient);
    public Task SaveChangesAsync()      => _ctx.SaveChangesAsync();
}
