using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _repo;

    public DesignationService(IDesignationRepository repo) => _repo = repo;

    public async Task<List<DesignationDto>> GetAllAsync(int orgId)
    {
        var designations = await _repo.GetAllByOrgAsync(orgId);
        return designations.Select(MapToDto).ToList();
    }

    public async Task<DesignationDto> GetByIdAsync(int orgId, int id)
    {
        var designation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Designation {id} not found");
        return MapToDto(designation);
    }

    public async Task<DesignationDto> CreateAsync(int orgId, CreateDesignationDto dto)
    {
        var designation = new Designation
        {
            OrganizationId = orgId,
            Title          = dto.Title,
            Code           = dto.Code,
            JobGradeId     = dto.JobGradeId,
            Description    = dto.Description,
            IsActive       = true
        };
        _repo.Add(designation);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(orgId, designation.Id);
        return MapToDto(created!);
    }

    public async Task<DesignationDto> UpdateAsync(int orgId, int id, UpdateDesignationDto dto)
    {
        var designation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Designation {id} not found");

        designation.Title       = dto.Title;
        designation.Code        = dto.Code;
        designation.JobGradeId  = dto.JobGradeId;
        designation.Description = dto.Description;
        designation.IsActive    = dto.IsActive;
        designation.UpdatedDate = DateTime.UtcNow;

        _repo.Update(designation);
        await _repo.SaveChangesAsync();

        var updated = await _repo.GetByIdAsync(orgId, id);
        return MapToDto(updated!);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var designation = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Designation {id} not found");
        _repo.Remove(designation);
        await _repo.SaveChangesAsync();
    }

    public async Task<List<JobGradeDto>> GetJobGradesAsync(int orgId)
    {
        var jobGrades = await _repo.GetJobGradesByOrgAsync(orgId);
        return jobGrades.Select(MapToDto).ToList();
    }

    public async Task<JobGradeDto> CreateJobGradeAsync(int orgId, CreateJobGradeDto dto)
    {
        var jobGrade = new JobGrade
        {
            OrganizationId  = orgId,
            Name            = dto.Name,
            Level           = dto.Level,
            MinAnnualSalary = dto.MinAnnualSalary,
            MaxAnnualSalary = dto.MaxAnnualSalary,
            IsActive        = true
        };
        _repo.AddJobGrade(jobGrade);
        await _repo.SaveChangesAsync();
        return MapToDto(jobGrade);
    }

    public async Task<JobGradeDto> UpdateJobGradeAsync(int orgId, int id, UpdateJobGradeDto dto)
    {
        var jobGrade = await _repo.GetJobGradeByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Job grade {id} not found");

        jobGrade.Name            = dto.Name;
        jobGrade.Level           = dto.Level;
        jobGrade.MinAnnualSalary = dto.MinAnnualSalary;
        jobGrade.MaxAnnualSalary = dto.MaxAnnualSalary;
        jobGrade.IsActive        = dto.IsActive;
        jobGrade.UpdatedDate     = DateTime.UtcNow;

        _repo.UpdateJobGrade(jobGrade);
        await _repo.SaveChangesAsync();
        return MapToDto(jobGrade);
    }

    public async Task DeleteJobGradeAsync(int orgId, int id)
    {
        var jobGrade = await _repo.GetJobGradeByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Job grade {id} not found");

        if (await _repo.IsJobGradeInUseAsync(id))
            throw new InvalidOperationException("Cannot delete a job grade that is assigned to designations.");

        _repo.RemoveJobGrade(jobGrade);
        await _repo.SaveChangesAsync();
    }

    private static DesignationDto MapToDto(Designation d) => new()
    {
        Id           = d.Id,
        Title        = d.Title,
        Code         = d.Code,
        JobGradeId   = d.JobGradeId,
        JobGradeName = d.JobGrade?.Name,
        Description  = d.Description,
        IsActive     = d.IsActive
    };

    private static JobGradeDto MapToDto(JobGrade g) => new()
    {
        Id              = g.Id,
        Name            = g.Name,
        Level           = g.Level,
        MinAnnualSalary = g.MinAnnualSalary,
        MaxAnnualSalary = g.MaxAnnualSalary,
        IsActive        = g.IsActive
    };
}
