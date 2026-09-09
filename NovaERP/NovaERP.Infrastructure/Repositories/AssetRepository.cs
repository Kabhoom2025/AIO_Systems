using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly NovaErpDbContext _ctx;

    public AssetRepository(NovaErpDbContext ctx) => _ctx = ctx;

    private IQueryable<Asset> Query() =>
        _ctx.Assets.Include(a => a.Category).Include(a => a.AssignedTo);

    public Task<List<Asset>> GetAllByOrgAsync(int orgId) =>
        Query().Where(a => a.OrganizationId == orgId).OrderBy(a => a.AssetCode).ToListAsync();

    public Task<Asset?> GetByIdAsync(int orgId, int id) =>
        Query().FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.Assets.AnyAsync(a => a.OrganizationId == orgId && a.AssetCode == code);

    public void Add(Asset asset)    => _ctx.Assets.Add(asset);
    public void Update(Asset asset) => _ctx.Assets.Update(asset);
    public void Remove(Asset asset) => _ctx.Assets.Remove(asset);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
