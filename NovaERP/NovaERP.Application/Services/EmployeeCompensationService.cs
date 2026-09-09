using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class EmployeeCompensationService : IEmployeeCompensationService
{
    private readonly IEmployeeCompensationRepository _repo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IValidator<CreateEmployeeCompensationDto> _createValidator;
    private readonly IValidator<UpdateEmployeeCompensationDto> _updateValidator;

    public EmployeeCompensationService(IEmployeeCompensationRepository repo, IEmployeeRepository employeeRepo,
        IValidator<CreateEmployeeCompensationDto> createValidator, IValidator<UpdateEmployeeCompensationDto> updateValidator)
    {
        _repo = repo;
        _employeeRepo = employeeRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<EmployeeCompensationDto>> GetAllAsync(int orgId)
    {
        var rows = await _repo.GetAllByOrgAsync(orgId);
        return rows.Select(ToDto).ToList();
    }

    public async Task<EmployeeCompensationDto> GetByIdAsync(int orgId, int id)
    {
        var row = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"EmployeeCompensation {id} not found");
        return ToDto(row);
    }

    public async Task<EmployeeCompensationDto> CreateAsync(int orgId, CreateEmployeeCompensationDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        _ = await _employeeRepo.GetByIdAsync(orgId, dto.EmployeeId)
            ?? throw new KeyNotFoundException($"Employee {dto.EmployeeId} not found");

        if (await _repo.EmployeeIdExistsAsync(orgId, dto.EmployeeId))
            throw new InvalidOperationException("This employee already has a compensation record.");

        var compensation = new EmployeeCompensation
        {
            OrganizationId = orgId,
            EmployeeId = dto.EmployeeId,
            BasicSalary = dto.BasicSalary,
            Hra = dto.Hra,
            OtherAllowances = dto.OtherAllowances,
            Deductions = dto.Deductions
        };

        _repo.Add(compensation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, compensation.Id) ?? compensation;
        return ToDto(reloaded);
    }

    public async Task<EmployeeCompensationDto> UpdateAsync(int orgId, int id, UpdateEmployeeCompensationDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var compensation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"EmployeeCompensation {id} not found");

        compensation.BasicSalary = dto.BasicSalary;
        compensation.Hra = dto.Hra;
        compensation.OtherAllowances = dto.OtherAllowances;
        compensation.Deductions = dto.Deductions;
        compensation.UpdatedDate = DateTime.UtcNow;

        _repo.Update(compensation);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, compensation.Id) ?? compensation;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var compensation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"EmployeeCompensation {id} not found");
        _repo.Remove(compensation);
        await _repo.SaveChangesAsync();
    }

    private static EmployeeCompensationDto ToDto(EmployeeCompensation c)
    {
        var gross = c.BasicSalary + c.Hra + c.OtherAllowances;
        return new EmployeeCompensationDto
        {
            Id = c.Id,
            EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : string.Empty,
            EmployeeCode = c.Employee?.EmployeeCode ?? string.Empty,
            BasicSalary = c.BasicSalary,
            Hra = c.Hra,
            OtherAllowances = c.OtherAllowances,
            Deductions = c.Deductions,
            GrossPay = gross,
            NetPay = gross - c.Deductions
        };
    }
}
