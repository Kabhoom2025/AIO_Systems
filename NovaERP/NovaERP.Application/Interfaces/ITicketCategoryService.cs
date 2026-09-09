using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface ITicketCategoryService
{
    Task<List<TicketCategoryDto>> GetAllAsync(int orgId);
    Task<TicketCategoryDto> GetByIdAsync(int orgId, int id);
    Task<TicketCategoryDto> CreateAsync(int orgId, CreateTicketCategoryDto dto);
    Task<TicketCategoryDto> UpdateAsync(int orgId, int id, UpdateTicketCategoryDto dto);
    Task DeleteAsync(int orgId, int id);
}
