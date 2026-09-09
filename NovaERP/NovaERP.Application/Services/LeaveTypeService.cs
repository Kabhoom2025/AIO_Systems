using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ILeaveTypeRepository _repo;
    private readonly IValidator<CreateLeaveTypeDto> _createValidator;
    private readonly IValidator<UpdateLeaveTypeDto> _updateValidator;

    public LeaveTypeService(ILeaveTypeRepository repo,
        IValidator<CreateLeaveTypeDto> createValidator, IValidator<UpdateLeaveTypeDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<LeaveTypeDto>> GetAllAsync(int orgId)
    {
        var types = await _repo.GetAllByOrgAsync(orgId);
        return types.Select(ToDto).ToList();
    }

    public async Task<LeaveTypeDto> GetByIdAsync(int orgId, int id)
    {
        var type = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveType {id} not found");
        return ToDto(type);
    }

    public async Task<LeaveTypeDto> CreateAsync(int orgId, CreateLeaveTypeDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Leave type code {code} already exists");

        var type = new LeaveType
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            DefaultDaysPerYear = dto.DefaultDaysPerYear,
            IsActive = dto.IsActive
        };

        _repo.Add(type);
        await _repo.SaveChangesAsync();
        return ToDto(type);
    }

    public async Task<LeaveTypeDto> UpdateAsync(int orgId, int id, UpdateLeaveTypeDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var type = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveType {id} not found");

        type.Name = dto.Name;
        type.DefaultDaysPerYear = dto.DefaultDaysPerYear;
        type.IsActive = dto.IsActive;
        type.UpdatedDate = DateTime.UtcNow;

        _repo.Update(type);
        await _repo.SaveChangesAsync();
        return ToDto(type);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var type = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"LeaveType {id} not found");
        _repo.Remove(type);
        await _repo.SaveChangesAsync();
    }

    private static LeaveTypeDto ToDto(LeaveType t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Code = t.Code,
        DefaultDaysPerYear = t.DefaultDaysPerYear,
        IsActive = t.IsActive
    };
}
