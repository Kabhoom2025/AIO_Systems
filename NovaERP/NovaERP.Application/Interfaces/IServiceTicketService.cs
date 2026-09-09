using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IServiceTicketService
{
    Task<List<ServiceTicketDto>> GetAllAsync(int orgId);
    Task<ServiceTicketDto> GetByIdAsync(int orgId, int id);
    Task<ServiceTicketDto> CreateAsync(int orgId, CreateServiceTicketDto dto);
    Task<ServiceTicketDto> UpdateAsync(int orgId, int id, UpdateServiceTicketDto dto);
    Task DeleteAsync(int orgId, int id);
    Task<ServiceTicketDto> AssignAsync(int orgId, int id, AssignTicketDto dto);
    Task<ServiceTicketDto> ResolveAsync(int orgId, int id, ResolveTicketDto dto);
    Task<ServiceTicketDto> CloseAsync(int orgId, int id);
    Task<ServiceTicketDto> ReopenAsync(int orgId, int id);
}
