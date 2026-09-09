using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class AssetCategoryRepository : IAssetCategoryRepository
{
    private readonly NovaErpDbContext _ctx;

    public AssetCategoryRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<AssetCategory>> GetAllByOrgAsync(int orgId) =>
        _ctx.AssetCategories.Where(c => c.OrganizationId == orgId).OrderBy(c => c.Code).ToListAsync();

    public Task<AssetCategory?> GetByIdAsync(int orgId, int id) =>
        _ctx.AssetCategories.FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId);

    public Task<bool> CodeExistsAsync(int orgId, string code) =>
        _ctx.AssetCategories.AnyAsync(c => c.OrganizationId == orgId && c.Code == code);

    public void Add(AssetCategory category)    => _ctx.AssetCategories.Add(category);
    public void Update(AssetCategory category) => _ctx.AssetCategories.Update(category);
    public void Remove(AssetCategory category) => _ctx.AssetCategories.Remove(category);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
