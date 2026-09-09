using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IProductionOrderService
{
    Task<List<ProductionOrderDto>> GetAllAsync(int orgId);
    Task<ProductionOrderDto> GetByIdAsync(int orgId, int id);
    Task<ProductionOrderDto> CreateAsync(int orgId, CreateProductionOrderDto dto);
    Task<ProductionOrderDto> UpdateAsync(int orgId, int id, UpdateProductionOrderDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<ProductionOrderDto> CompleteAsync(int orgId, int id);
    Task<ProductionOrderDto> CancelAsync(int orgId, int id);
}
