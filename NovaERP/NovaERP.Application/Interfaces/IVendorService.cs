using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IVendorService
{
    Task<List<VendorDto>> GetAllAsync(int orgId);
    Task<VendorDto> GetByIdAsync(int orgId, int id);
    Task<VendorDto> CreateAsync(int orgId, CreateVendorDto dto);
    Task<VendorDto> UpdateAsync(int orgId, int id, UpdateVendorDto dto);
    Task DeleteAsync(int orgId, int id);
}
