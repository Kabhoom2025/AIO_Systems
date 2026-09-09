using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly NovaErpDbContext _ctx;

    public ProductRepository(NovaErpDbContext ctx) => _ctx = ctx;

    public Task<List<Product>> GetAllByOrgAsync(int orgId) =>
        _ctx.Products
            .Where(p => p.OrganizationId == orgId)
            .OrderBy(p => p.Sku)
            .ToListAsync();

    public Task<Product?> GetByIdAsync(int orgId, int id) =>
        _ctx.Products.FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == orgId);

    public Task<bool> SkuExistsAsync(int orgId, string sku) =>
        _ctx.Products.AnyAsync(p => p.OrganizationId == orgId && p.Sku == sku);

    public async Task<Dictionary<int, decimal>> GetOnHandQuantitiesByOrgAsync(int orgId) =>
        await _ctx.StockMovements
            .Where(m => m.OrganizationId == orgId)
            .GroupBy(m => m.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(m => m.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Total);

    public Task<decimal> GetOnHandQuantityAsync(int orgId, int productId) =>
        _ctx.StockMovements
            .Where(m => m.OrganizationId == orgId && m.ProductId == productId)
            .SumAsync(m => m.Quantity);

    public void Add(Product product)    => _ctx.Products.Add(product);
    public void Update(Product product) => _ctx.Products.Update(product);
    public void Remove(Product product) => _ctx.Products.Remove(product);
    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}
