using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class ProjectTaskService : IProjectTaskService
{
    private readonly IProjectTaskRepository _repo;
    private readonly IProjectRepository _projectRepo;
    private readonly IValidator<CreateProjectTaskDto> _createValidator;
    private readonly IValidator<UpdateProjectTaskDto> _updateValidator;

    public ProjectTaskService(IProjectTaskRepository repo, IProjectRepository projectRepo,
        IValidator<CreateProjectTaskDto> createValidator, IValidator<UpdateProjectTaskDto> updateValidator)
    {
        _repo = repo;
        _projectRepo = projectRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<ProjectTaskDto>> GetAllAsync(int orgId)
    {
        var tasks = await _repo.GetAllByOrgAsync(orgId);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<ProjectTaskDto> GetByIdAsync(int orgId, int id)
    {
        var task = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProjectTask {id} not found");
        return ToDto(task);
    }

    public async Task<ProjectTaskDto> CreateAsync(int orgId, CreateProjectTaskDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        _ = await _projectRepo.GetByIdAsync(orgId, dto.ProjectId)
            ?? throw new KeyNotFoundException($"Project {dto.ProjectId} not found");

        var task = new ProjectTask
        {
            OrganizationId = orgId,
            ProjectId = dto.ProjectId,
            AssignedToId = dto.AssignedToId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            StartDate = dto.StartDate,
            DueDate = dto.DueDate
        };

        _repo.Add(task);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, task.Id) ?? task;
        return ToDto(reloaded);
    }

    public async Task<ProjectTaskDto> UpdateAsync(int orgId, int id, UpdateProjectTaskDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var task = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProjectTask {id} not found");

        task.AssignedToId = dto.AssignedToId;
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.Status = dto.Status;
        task.StartDate = dto.StartDate;
        task.DueDate = dto.DueDate;
        task.UpdatedDate = DateTime.UtcNow;

        _repo.Update(task);
        await _repo.SaveChangesAsync();

        var reloaded = await _repo.GetByIdAsync(orgId, task.Id) ?? task;
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var task = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"ProjectTask {id} not found");
        _repo.Remove(task);
        await _repo.SaveChangesAsync();
    }

    private static ProjectTaskDto ToDto(ProjectTask t) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        ProjectName = t.Project?.Name ?? string.Empty,
        AssignedToId = t.AssignedToId,
        AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : null,
        Title = t.Title,
        Description = t.Description,
        Priority = t.Priority,
        Status = t.Status,
        StartDate = t.StartDate,
        DueDate = t.DueDate
    };
}
