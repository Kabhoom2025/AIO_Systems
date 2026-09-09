using NovaERP.Domain.Entities;

namespace NovaERP.Application.Interfaces;

public interface IStockTransferRepository
{
    Task<List<StockTransfer>> GetAllByOrgAsync(int orgId);
    void Add(StockTransfer transfer);
    Task SaveChangesAsync();
}
