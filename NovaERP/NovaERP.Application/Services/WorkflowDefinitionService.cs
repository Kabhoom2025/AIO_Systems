using AutoMapper;
using FluentValidation;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class WorkflowDefinitionService : IWorkflowDefinitionService
{
    private readonly IWorkflowDefinitionRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateWorkflowDefinitionDto> _createValidator;
    private readonly IValidator<UpdateWorkflowDefinitionDto> _updateValidator;

    public WorkflowDefinitionService(IWorkflowDefinitionRepository repo, IMapper mapper,
        IValidator<CreateWorkflowDefinitionDto> createValidator, IValidator<UpdateWorkflowDefinitionDto> updateValidator)
    {
        _repo = repo;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<List<WorkflowDefinitionDto>> GetAllAsync(int orgId)
    {
        var definitions = await _repo.GetAllByOrgAsync(orgId);
        return definitions.Select(ToDto).ToList();
    }

    public async Task<WorkflowDefinitionDto> GetByIdAsync(int orgId, int id)
    {
        var definition = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"WorkflowDefinition {id} not found");
        return ToDto(definition);
    }

    public async Task<WorkflowDefinitionDto> CreateAsync(int orgId, CreateWorkflowDefinitionDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var definition = new WorkflowDefinition
        {
            OrganizationId = orgId,
            Name = dto.Name,
            EntityType = dto.EntityType,
            IsActive = dto.IsActive,
            Steps = dto.Steps.Select(s => new WorkflowStepDefinition
            {
                StepOrder = s.StepOrder,
                Name = s.Name,
                ApproverRoleId = s.ApproverRoleId,
                MinAmount = s.MinAmount
            }).ToList()
        };

        _repo.Add(definition);
        await _repo.SaveChangesAsync();
        return ToDto(definition);
    }

    public async Task<WorkflowDefinitionDto> UpdateAsync(int orgId, int id, UpdateWorkflowDefinitionDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var definition = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"WorkflowDefinition {id} not found");

        definition.Name = dto.Name;
        definition.EntityType = dto.EntityType;
        definition.IsActive = dto.IsActive;
        definition.UpdatedDate = DateTime.UtcNow;

        // Steps are owned by the definition and replaced wholesale on update.
        definition.Steps.Clear();
        foreach (var s in dto.Steps)
        {
            definition.Steps.Add(new WorkflowStepDefinition
            {
                StepOrder = s.StepOrder,
                Name = s.Name,
                ApproverRoleId = s.ApproverRoleId,
                MinAmount = s.MinAmount
            });
        }

        _repo.Update(definition);
        await _repo.SaveChangesAsync();
        return ToDto(definition);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var definition = await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"WorkflowDefinition {id} not found");

        // A definition with live instances is not hard-deletable — instances reference it
        // directly (no snapshot table for the definition itself), so deleting it would orphan
        // history. Deactivate instead (IsActive = false) via Update in that case.
        if (await _repo.HasInstancesAsync(id))
            throw new InvalidOperationException(
                "This workflow definition has existing workflow instances and cannot be deleted. Set IsActive to false instead.");

        _repo.Remove(definition);
        await _repo.SaveChangesAsync();
    }

    private static WorkflowDefinitionDto ToDto(WorkflowDefinition d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        EntityType = d.EntityType,
        IsActive = d.IsActive,
        Steps = d.Steps.OrderBy(s => s.StepOrder).Select(s => new WorkflowStepDefinitionDto
        {
            Id = s.Id,
            StepOrder = s.StepOrder,
            Name = s.Name,
            ApproverRoleId = s.ApproverRoleId,
            ApproverRoleName = s.ApproverRole?.Name,
            MinAmount = s.MinAmount
        }).ToList()
    };
}
