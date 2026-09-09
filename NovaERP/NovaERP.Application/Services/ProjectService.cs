using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repo;
    private readonly IValidator<CreateProjectDto> _createValidator;
    private readonly IValidator<UpdateProjectDto> _updateValidator;

    public ProjectService(IProjectRepository repo,
        IValidator<CreateProjectDto> createValidator, IValidator<UpdateProjectDto> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ProjectDto>> GetAllAsync(int orgId)
    {
        var projects = await _repo.GetAllByOrgAsync(orgId);
        var counts = await _repo.GetTaskCountsByOrgAsync(orgId);
        return projects.Select(p =>
        {
            var (total, completed) = counts.GetValueOrDefault(p.Id, (0, 0));
            return ToDto(p, total, completed);
        }).ToList();
    }

    public async Task<ProjectDto> GetByIdAsync(int orgId, int id)
    {
        var project = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Project {id} not found");
        var (total, completed) = await _repo.GetTaskCountsAsync(orgId, id);
        return ToDto(project, total, completed);
    }

    public async Task<ProjectDto> CreateAsync(int orgId, CreateProjectDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var code = dto.Code.ToUpperInvariant();
        if (await _repo.CodeExistsAsync(orgId, code))
            throw new InvalidOperationException($"Project code {code} already exists");

        var project = new Project
        {
            OrganizationId = orgId,
            Code = code,
            Name = dto.Name,
            Description = dto.Description,
            ManagerId = dto.ManagerId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            Status = "Planning"
        };

        _repo.Add(project);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, project.Id) ?? project;
        return ToDto(reloaded, 0, 0);
    }

    public async Task<ProjectDto> UpdateAsync(int orgId, int id, UpdateProjectDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var project = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Project {id} not found");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.ManagerId = dto.ManagerId;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Budget = dto.Budget;
        project.Status = dto.Status;
        project.UpdatedDate = DateTime.UtcNow;

        _repo.Update(project);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, project.Id) ?? project;
        var (total, completed) = await _repo.GetTaskCountsAsync(orgId, id);
        return ToDto(reloaded, total, completed);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var project = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Project {id} not found");

        if (await _repo.HasTasksAsync(orgId, id))
            throw new InvalidOperationException("Cannot delete a project that still has tasks.");

        _repo.Remove(project);
        await _repo.SaveChangesAsync();
    }

    private static ProjectDto ToDto(Project p, int taskCount, int completedTaskCount) => new()
    {
        Id = p.Id,
        Code = p.Code,
        Name = p.Name,
        Description = p.Description,
        ManagerId = p.ManagerId,
        ManagerName = p.Manager != null ? $"{p.Manager.FirstName} {p.Manager.LastName}" : string.Empty,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Budget = p.Budget,
        Status = p.Status,
        TaskCount = taskCount,
        CompletedTaskCount = completedTaskCount
    };
}
