using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo) => _repo = repo;

    public async Task<List<DepartmentDto>> GetAllAsync(int orgId)
    {
        var departments = await _repo.GetAllByOrgAsync(orgId);
        var counts      = await _repo.GetEmployeeCountsAsync(orgId);
        return departments
            .Select(d => MapToDto(d, counts.GetValueOrDefault(d.Id)))
            .ToList();
    }

    public async Task<DepartmentDto> GetByIdAsync(int orgId, int id)
    {
        var department = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Department {id} not found");
        var count = await _repo.GetEmployeeCountAsync(id);
        return MapToDto(department, count);
    }

    public async Task<DepartmentDto> CreateAsync(int orgId, CreateDepartmentDto dto)
    {
        var department = new Department
        {
            OrganizationId = orgId,
            BranchId       = dto.BranchId,
            ParentId       = dto.ParentId,
            Name           = dto.Name,
            Code           = dto.Code,
            Description    = dto.Description,
            HeadEmployeeId = dto.HeadEmployeeId,
            CostCenter     = dto.CostCenter,
            IsActive       = true
        };
        _repo.Add(department);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(orgId, department.Id);
        return MapToDto(created!, 0);
    }

    public async Task<DepartmentDto> UpdateAsync(int orgId, int id, UpdateDepartmentDto dto)
    {
        var department = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Department {id} not found");

        if (dto.ParentId == id)
            throw new InvalidOperationException("A department cannot be its own parent.");

        department.BranchId       = dto.BranchId;
        department.ParentId       = dto.ParentId;
        department.Name           = dto.Name;
        department.Code           = dto.Code;
        department.Description    = dto.Description;
        department.HeadEmployeeId = dto.HeadEmployeeId;
        department.CostCenter     = dto.CostCenter;
        department.IsActive       = dto.IsActive;
        department.UpdatedDate    = DateTime.UtcNow;

        _repo.Update(department);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetByIdAsync(orgId, id);
        var count   = await _repo.GetEmployeeCountAsync(id);
        return MapToDto(updated!, count);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var department = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Department {id} not found");

        var employeeCount = await _repo.GetEmployeeCountAsync(id);
        if (employeeCount > 0)
            throw new InvalidOperationException("Cannot delete a department that has employees assigned to it.");
        if (department.Children.Count > 0)
            throw new InvalidOperationException("Cannot delete a department that has sub-departments.");

        _repo.Remove(department);
        await _repo.SaveChangesAsync();
    }

    private static DepartmentDto MapToDto(Department d, int employeeCount) => new()
    {
        Id               = d.Id,
        BranchId         = d.BranchId,
        BranchName       = d.Branch?.Name,
        ParentId         = d.ParentId,
        ParentName       = d.Parent?.Name,
        Name             = d.Name,
        Code             = d.Code,
        Description      = d.Description,
        HeadEmployeeId   = d.HeadEmployeeId,
        HeadEmployeeName = d.HeadEmployee?.FullName,
        CostCenter       = d.CostCenter,
        EmployeeCount    = employeeCount,
        IsActive         = d.IsActive
    };
}
