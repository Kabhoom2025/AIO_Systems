using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(int orgId);
    Task<ProductDto> GetByIdAsync(int orgId, int id);
    Task<ProductDto> CreateAsync(int orgId, CreateProductDto dto);
    Task<ProductDto> UpdateAsync(int orgId, int id, UpdateProductDto dto);
    Task DeleteAsync(int orgId, int id);
}
