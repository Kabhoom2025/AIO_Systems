using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly PharmacyDbContext _ctx;

    public MedicineRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Medicine>> GetAllByOrgAsync(int orgId) =>
        _ctx.Medicines
            .Include(m => m.Batches)
            .Where(m => m.OrganizationId == orgId)
            .OrderBy(m => m.Name)
            .ToListAsync();

    public Task<Medicine?> GetByIdAsync(int id) =>
        _ctx.Medicines
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.Id == id);

    public Task<Medicine?> GetByCodeAsync(int orgId, string code) =>
        _ctx.Medicines
            .Include(m => m.Batches)
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && (m.Barcode == code || m.Sku == code));

    public Task<List<MedicineBatch>> GetExpiryAlertsAsync(int orgId, int daysThreshold) =>
        _ctx.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => b.Medicine.OrganizationId == orgId
                     && b.CurrentQuantity > 0
                     && b.ExpiryDate <= DateTime.UtcNow.AddDays(daysThreshold))
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

    public void Add(Medicine medicine)          => _ctx.Medicines.Add(medicine);
    public void Update(Medicine medicine)       => _ctx.Medicines.Update(medicine);
    public void Remove(Medicine medicine)       => _ctx.Medicines.Remove(medicine);
    public void AddBatch(MedicineBatch batch)   => _ctx.MedicineBatches.Add(batch);
    public Task SaveChangesAsync()              => _ctx.SaveChangesAsync();
}
