using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly PharmacyDbContext _ctx;

    public DoctorRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Doctor>> GetAllByOrgAsync(int orgId) =>
        _ctx.Doctors
            .Where(d => d.OrganizationId == orgId)
            .OrderBy(d => d.Name)
            .ToListAsync();

    public Task<Doctor?> GetByIdAsync(int id) =>
        _ctx.Doctors.FirstOrDefaultAsync(d => d.Id == id);

    public void Add(Doctor doctor)    => _ctx.Doctors.Add(doctor);
    public void Update(Doctor doctor) => _ctx.Doctors.Update(doctor);
    public void Remove(Doctor doctor) => _ctx.Doctors.Remove(doctor);
    public Task SaveChangesAsync()    => _ctx.SaveChangesAsync();
}
