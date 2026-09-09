using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IStockTransferService
{
    Task<List<StockTransferDto>> GetAllAsync(int orgId);
    Task<StockTransferDto> CreateAsync(int orgId, CreateStockTransferDto dto);
}
