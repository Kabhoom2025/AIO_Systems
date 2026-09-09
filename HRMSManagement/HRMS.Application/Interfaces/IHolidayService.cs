using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IHolidayService
{
    Task<List<HolidayDto>> GetAllAsync(int orgId, int? year);
    Task<List<HolidayDto>> GetUpcomingAsync(int orgId);
    Task<HolidayDto> GetByIdAsync(int orgId, int id);
    Task<HolidayDto> CreateAsync(int orgId, CreateHolidayDto dto);
    Task<HolidayDto> UpdateAsync(int orgId, int id, UpdateHolidayDto dto);
    Task DeleteAsync(int orgId, int id);
}
