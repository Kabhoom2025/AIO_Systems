using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IValidator<CreateEmployeeDto> _createValidator;
    private readonly IValidator<UpdateEmployeeDto> _updateValidator;

    public EmployeeService(IEmployeeRepository repo, IDepartmentRepository departmentRepo,
        IValidator<CreateEmployeeDto> createValidator, IValidator<UpdateEmployeeDto> updateValidator)
    {
        _repo = repo;
        _departmentRepo = departmentRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<EmployeeDto>> GetAllAsync(int orgId)
    {
        var employees = await _repo.GetAllByOrgAsync(orgId);
        return employees.Select(ToDto).ToList();
    }

    public async Task<EmployeeDto> GetByIdAsync(int orgId, int id)
    {
        var employee = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Employee {id} not found");
        return ToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(int orgId, CreateEmployeeDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        _ = await _departmentRepo.GetByIdAsync(orgId, dto.DepartmentId)
            ?? throw new KeyNotFoundException($"Department {dto.DepartmentId} not found");

        var employee = new Employee
        {
            OrganizationId = orgId,
            DepartmentId = dto.DepartmentId,
            UserId = dto.UserId,
            ReportingManagerId = dto.ReportingManagerId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            JobTitle = dto.JobTitle,
            EmploymentType = dto.EmploymentType,
            DateOfJoining = dto.DateOfJoining,
            Status = "Active"
        };

        _repo.Add(employee);
        await _repo.SaveChangesAsync();

        // EmployeeCode depends on the generated Id, so it's set in a second save — same
        // scheme as SalesOrder.OrderNumber/ProductionOrder.MoNumber.
        employee.EmployeeCode = $"EMP-{employee.Id:D5}";
        _repo.Update(employee);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, employee.Id) ?? employee;
        return ToDto(reloaded);
    }

    public async Task<EmployeeDto> UpdateAsync(int orgId, int id, UpdateEmployeeDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var employee = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Employee {id} not found");

        if (employee.Status == "Terminated")
            throw new InvalidOperationException("Terminated employees cannot be edited.");

        employee.UserId = dto.UserId;
        employee.ReportingManagerId = dto.ReportingManagerId;
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.Gender = dto.Gender;
        employee.JobTitle = dto.JobTitle;
        employee.EmploymentType = dto.EmploymentType;
        employee.DateOfJoining = dto.DateOfJoining;
        employee.UpdatedDate = DateTime.UtcNow;

        _repo.Update(employee);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, employee.Id) ?? employee;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var employee = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Employee {id} not found");

        if (employee.Status == "Terminated")
            throw new InvalidOperationException("Terminated employees cannot be deleted.");

        _repo.Remove(employee);
        await _repo.SaveChangesAsync();
    }

    public async Task<EmployeeDto> TerminateAsync(int orgId, int id)
    {
        var employee = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Employee {id} not found");

        if (employee.Status == "Terminated")
            throw new InvalidOperationException("Employee is already terminated.");

        employee.Status = "Terminated";
        employee.TerminationDate = DateTime.UtcNow.Date;
        employee.UpdatedDate = DateTime.UtcNow;
        _repo.Update(employee);
        await _repo.SaveChangesAsync();

        return ToDto(employee);
    }

    private static EmployeeDto ToDto(Employee e) => new()
    {
        Id = e.Id,
        EmployeeCode = e.EmployeeCode,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name ?? string.Empty,
        UserId = e.UserId,
        UserName = e.User?.Name,
        ReportingManagerId = e.ReportingManagerId,
        ReportingManagerName = e.ReportingManager != null ? $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}" : null,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        DateOfBirth = e.DateOfBirth,
        Gender = e.Gender,
        JobTitle = e.JobTitle,
        EmploymentType = e.EmploymentType,
        DateOfJoining = e.DateOfJoining,
        TerminationDate = e.TerminationDate,
        Status = e.Status
    };
}
