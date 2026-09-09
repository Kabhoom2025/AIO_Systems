using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ILeadService
{
    Task<List<LeadDto>> GetAllAsync(int orgId);
    Task<LeadDto> GetByIdAsync(int orgId, int id);
    Task<LeadDto> CreateAsync(int orgId, CreateLeadDto dto);
    Task<LeadDto> UpdateAsync(int orgId, int id, UpdateLeadDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<ConvertLeadResultDto> ConvertAsync(int orgId, int id, ConvertLeadDto dto);
}
