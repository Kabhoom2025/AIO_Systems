using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class TaxCodeRepository : ITaxCodeRepository
{
    private readonly NovaErpDbContext _ctx;

    public TaxCodeRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<TaxCode>> GetAllByOrgAsync(int orgId) =>
        _ctx.TaxCodes
            .Include(t => t.Components)
            .Where(t => t.OrganizationId == orgId)
            .OrderBy(t => t.Code)
            .ToListAsync();

    public Task<TaxCode?> GetByIdAsync(int orgId, int id) =>
        _ctx.TaxCodes
            .Include(t => t.Components)
            .FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.TaxCodes.AnyAsync(t => t.OrganizationId == orgId && t.Code == code);

    public void Add(TaxCode taxCode)    => _ctx.TaxCodes.Add(taxCode);
    public void Update(TaxCode taxCode) => _ctx.TaxCodes.Update(taxCode);
    public void Remove(TaxCode taxCode) => _ctx.TaxCodes.Remove(taxCode);
    public Task SaveChangesAsync()      => _ctx.SaveChangesAsync();
}
