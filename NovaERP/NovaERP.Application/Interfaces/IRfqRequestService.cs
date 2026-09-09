using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IRfqRequestService
{
    Task<List<RfqRequestDto>> GetAllAsync(int orgId);
    Task<RfqRequestDto> GetByIdAsync(int orgId, int id);
    Task<RfqRequestDto> CreateAsync(int orgId, CreateRfqRequestDto dto);
    Task<RfqRequestDto> UpdateAsync(int orgId, int id, UpdateRfqRequestDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<RfqRequestDto> SendAsync(int orgId, int id);
    Task<RfqRequestDto> RecordQuoteAsync(int orgId, int id, RecordRfqQuoteDto dto);
    Task<RfqRequestDto> CloseAsync(int orgId, int id, CloseRfqRequestDto dto);
    Task<RfqRequestDto> CancelAsync(int orgId, int id);
}
