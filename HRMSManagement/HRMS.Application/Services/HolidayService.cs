using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class HolidayService : IHolidayService
{
    private readonly IHolidayRepository _repo;

    public HolidayService(IHolidayRepository repo) => _repo = repo;

    public async Task<List<HolidayDto>> GetAllAsync(int orgId, int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var holidays   = await _repo.GetAllByOrgAsync(orgId, targetYear);
        return holidays.Select(MapToDto).ToList();
    }

    public async Task<List<HolidayDto>> GetUpcomingAsync(int orgId)
    {
        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var holidays = await _repo.GetUpcomingAsync(orgId, today, 10);
        return holidays.Select(MapToDto).ToList();
    }

    public async Task<HolidayDto> GetByIdAsync(int orgId, int id)
    {
        var holiday = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Holiday {id} not found");
        return MapToDto(holiday);
    }

    public async Task<HolidayDto> CreateAsync(int orgId, CreateHolidayDto dto)
    {
        var holiday = new Holiday
        {
            OrganizationId = orgId,
            BranchId       = dto.BranchId,
            Name           = dto.Name,
            Date           = dto.Date,
            Type           = dto.Type,
            Description    = dto.Description
        };
        _repo.Add(holiday);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(orgId, holiday.Id);
        return MapToDto(created!);
    }

    public async Task<HolidayDto> UpdateAsync(int orgId, int id, UpdateHolidayDto dto)
    {
        var holiday = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Holiday {id} not found");

        holiday.BranchId    = dto.BranchId;
        holiday.Name        = dto.Name;
        holiday.Date        = dto.Date;
        holiday.Type        = dto.Type;
        holiday.Description = dto.Description;
        holiday.UpdatedDate = DateTime.UtcNow;

        _repo.Update(holiday);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetByIdAsync(orgId, id);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var holiday = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Holiday {id} not found");
        _repo.Remove(holiday);
        await _repo.SaveChangesAsync();
    }

    private static HolidayDto MapToDto(Holiday h) => new()
    {
        Id          = h.Id,
        BranchId    = h.BranchId,
        BranchName  = h.Branch?.Name,
        Name        = h.Name,
        Date        = h.Date,
        Type        = h.Type,
        Description = h.Description
    };
}
