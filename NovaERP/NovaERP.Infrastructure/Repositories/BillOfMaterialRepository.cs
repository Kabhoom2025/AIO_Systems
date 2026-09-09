using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class BillOfMaterialRepository : IBillOfMaterialRepository
{
    private readonly NovaErpDbContext _ctx;

    public BillOfMaterialRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<BillOfMaterial> Query() =>
        _ctx.BillOfMaterials
            .Include(b => b.Product)
            .Include(b => b.Components).ThenInclude(c => c.ComponentProduct);

    public Task<List<BillOfMaterial>> GetAllByOrgAsync(int orgId) =>
        Query().Where(b => b.OrganizationId == orgId).OrderBy(b => b.Product.Name).ToListAsync();

    public Task<BillOfMaterial?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(b => b.Id == id && b.OrganizationId == orgId);

    public Task<BillOfMaterial?> GetByProductIdAsync(int orgId, int productId) =>
        Query().FirstOrDefaultAsync(b => b.ProductId == productId && b.OrganizationId == orgId);

    public Task<bool> ExistsForProductAsync(int orgId, int productId) =>
        _ctx.BillOfMaterials.AnyAsync(b => b.OrganizationId == orgId && b.ProductId == productId);

    public void Add(BillOfMaterial bom)    => _ctx.BillOfMaterials.Add(bom);
    public void Update(BillOfMaterial bom) => _ctx.BillOfMaterials.Update(bom);
    public void Remove(BillOfMaterial bom) => _ctx.BillOfMaterials.Remove(bom);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
