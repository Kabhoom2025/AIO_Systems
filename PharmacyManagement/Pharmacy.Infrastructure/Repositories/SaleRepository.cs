using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly PharmacyDbContext _ctx;

    public SaleRepository(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<Sale>> GetAllByOrgAsync(int orgId) =>
        _ctx.Sales
            .Include(s => s.Branch)
            .Include(s => s.Customer)
            .Include(s => s.Patient)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .Where(s => s.OrganizationId == orgId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

    public Task<List<Sale>> GetByPatientAsync(int patientId) =>
        _ctx.Sales
            .Include(s => s.Branch)
            .Include(s => s.Customer)
            .Include(s => s.Patient)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .Where(s => s.PatientId == patientId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();

    public Task<Sale?> GetByIdAsync(int id) =>
        _ctx.Sales
            .Include(s => s.Branch)
            .Include(s => s.Customer)
            .Include(s => s.Patient)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<Medicine?> GetMedicineWithBatchesAsync(int medicineId) =>
        _ctx.Medicines
            .Include(m => m.Batches.Where(b => b.CurrentQuantity > 0).OrderBy(b => b.ExpiryDate))
            .FirstOrDefaultAsync(m => m.Id == medicineId);

    public Task<Customer?> GetCustomerAsync(int customerId) =>
        _ctx.Customers.FirstOrDefaultAsync(c => c.Id == customerId);

    public void Add(Sale sale)      => _ctx.Sales.Add(sale);
    public Task SaveChangesAsync()  => _ctx.SaveChangesAsync();
}
