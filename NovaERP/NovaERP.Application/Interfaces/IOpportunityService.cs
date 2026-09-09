using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IOpportunityService
{
    Task<List<OpportunityDto>> GetAllAsync(int orgId);
    Task<OpportunityDto> GetByIdAsync(int orgId, int id);
    Task<OpportunityDto> CreateAsync(int orgId, CreateOpportunityDto dto);
    Task<OpportunityDto> UpdateAsync(int orgId, int id, UpdateOpportunityDto dto);
    Task DeleteAsync(int orgId, int id);
}
