using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IContactService
{
    Task<List<ContactDto>> GetAllAsync(int orgId);
    Task<ContactDto> GetByIdAsync(int orgId, int id);
    Task<ContactDto> CreateAsync(int orgId, CreateContactDto dto);
    Task<ContactDto> UpdateAsync(int orgId, int id, UpdateContactDto dto);
    Task DeleteAsync(int orgId, int id);
}
