using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IPosSaleService
{
    Task<List<PosSaleDto>> GetAllAsync(int orgId);
    Task<PosSaleDto> GetByIdAsync(int orgId, int id);
    Task<PosSaleDto> CreateAsync(int orgId, CreatePosSaleDto dto);
    Task<PosSaleDto> UpdateAsync(int orgId, int id, UpdatePosSaleDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<PosSaleDto> CompleteAsync(int orgId, int id, CompletePosSaleDto dto);
    Task<PosSaleDto> RefundAsync(int orgId, int id);
    Task<PosSaleDto> CancelAsync(int orgId, int id);
}
