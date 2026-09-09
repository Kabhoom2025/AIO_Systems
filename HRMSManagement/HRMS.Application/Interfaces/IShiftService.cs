using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IShiftService
{
    Task<List<ShiftDto>> GetAllAsync(int orgId);
    Task<ShiftDto> GetByIdAsync(int orgId, int id);
    Task<ShiftDto> CreateAsync(int orgId, CreateShiftDto dto);
    Task<ShiftDto> UpdateAsync(int orgId, int id, UpdateShiftDto dto);
    Task DeleteAsync(int orgId, int id);
}
