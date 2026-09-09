using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetAllByOrgAsync(int orgId);
    Task<Product?> GetByIdAsync(int orgId, int id);
    Task<bool> SkuExistsAsync(int orgId, string sku);

    /// <summary>Sum of Quantity per ProductId, for the whole org in one query — avoids N+1
    /// when building the Products list, each row needing its own on-hand total.</summary>
    Task<Dictionary<int, decimal>> GetOnHandQuantitiesByOrgAsync(int orgId);
    Task<decimal> GetOnHandQuantityAsync(int orgId, int productId);

    void Add(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task SaveChangesAsync();
}
