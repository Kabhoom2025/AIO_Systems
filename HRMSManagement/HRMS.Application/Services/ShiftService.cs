using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _repo;

    public ShiftService(IShiftRepository repo) => _repo = repo;

    public async Task<List<ShiftDto>> GetAllAsync(int orgId)
    {
        var shifts = await _repo.GetAllByOrgAsync(orgId);
        return shifts.Select(MapToDto).ToList();
    }

    public async Task<ShiftDto> GetByIdAsync(int orgId, int id)
    {
        var shift = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shift {id} not found");
        return MapToDto(shift);
    }

    public async Task<ShiftDto> CreateAsync(int orgId, CreateShiftDto dto)
    {
        var shift = new Shift
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            Code           = dto.Code,
            StartTime      = dto.StartTime,
            EndTime        = dto.EndTime,
            BreakMinutes   = dto.BreakMinutes,
            GraceMinutes   = dto.GraceMinutes,
            IsNightShift   = dto.IsNightShift,
            WeeklyOffDays  = dto.WeeklyOffDays,
            IsActive       = true
        };
        _repo.Add(shift);
        await _repo.SaveChangesAsync();
        return MapToDto(shift);
    }

    public async Task<ShiftDto> UpdateAsync(int orgId, int id, UpdateShiftDto dto)
    {
        var shift = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shift {id} not found");

        shift.Name          = dto.Name;
        shift.Code          = dto.Code;
        shift.StartTime     = dto.StartTime;
        shift.EndTime       = dto.EndTime;
        shift.BreakMinutes  = dto.BreakMinutes;
        shift.GraceMinutes  = dto.GraceMinutes;
        shift.IsNightShift  = dto.IsNightShift;
        shift.WeeklyOffDays = dto.WeeklyOffDays;
        shift.IsActive      = dto.IsActive;
        shift.UpdatedDate   = DateTime.UtcNow;

        _repo.Update(shift);
        await _repo.SaveChangesAsync();
        return MapToDto(shift);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var shift = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Shift {id} not found");
        _repo.Remove(shift);
        await _repo.SaveChangesAsync();
    }

    private static ShiftDto MapToDto(Shift s) => new()
    {
        Id            = s.Id,
        Name          = s.Name,
        Code          = s.Code,
        StartTime     = s.StartTime,
        EndTime       = s.EndTime,
        BreakMinutes  = s.BreakMinutes,
        GraceMinutes  = s.GraceMinutes,
        IsNightShift  = s.IsNightShift,
        WeeklyOffDays = s.WeeklyOffDays,
        IsActive      = s.IsActive
    };
}
