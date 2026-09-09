using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IPayRunService
{
    Task<List<PayRunDto>> GetAllAsync(int orgId);
    Task<PayRunDto> GetByIdAsync(int orgId, int id);
    Task<PayRunDto> CreateAsync(int orgId, CreatePayRunDto dto);
    Task<PayRunDto> UpdateAsync(int orgId, int id, UpdatePayRunDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<PayRunDto> ProcessAsync(int orgId, int id);
    Task<PayRunDto> PayAsync(int orgId, int id, PayPayRunDto dto);
    Task<PayRunDto> CancelAsync(int orgId, int id);
}
