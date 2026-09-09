using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync(int orgId);
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierDto> CreateAsync(int orgId, CreateSupplierDto dto);
    Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto dto);
    Task DeleteAsync(int id);
}
