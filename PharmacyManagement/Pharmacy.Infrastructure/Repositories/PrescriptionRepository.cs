using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly PharmacyDbContext _ctx;

    public PrescriptionRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Prescription>> GetAllByOrgAsync(int orgId) =>
        _ctx.Prescriptions
            .Include(p => p.Items)
            .Where(p => p.OrganizationId == orgId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

    public Task<Prescription?> GetByIdAsync(int id) =>
        _ctx.Prescriptions
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

    public void Add(Prescription prescription)    => _ctx.Prescriptions.Add(prescription);
    public void Update(Prescription prescription) => _ctx.Prescriptions.Update(prescription);
    public Task SaveChangesAsync()                => _ctx.SaveChangesAsync();
}
