using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class WorkflowDesignService : IWorkflowDesignService
{
    private readonly IWorkflowRepository _repo;

    public WorkflowDesignService(IWorkflowRepository repo) => _repo = repo;

    public async Task<List<WorkflowDefinitionDto>> GetAllAsync(int orgId)
    {
        var definitions = await _repo.GetAllAsync(orgId);
        var result = new List<WorkflowDefinitionDto>();
        foreach (var d in definitions)
        {
            var draft = await _repo.GetDraftVersionAsync(d.Id);
            result.Add(MapToDto(d, draft));
        }
        return result;
    }

    public async Task<WorkflowDefinitionDto> GetByIdAsync(int orgId, int id)
    {
        var definition = await GetOwnedAsync(orgId, id);
        var draft = await _repo.GetDraftVersionAsync(definition.Id);
        return MapToDto(definition, draft);
    }

    public async Task<WorkflowDefinitionDto> CreateAsync(int orgId, CreateWorkflowDefinitionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Name is required.");

        var triggerLabel = GetTriggerTypes().FirstOrDefault(t => t.Key == dto.TriggerType)?.Label
            ?? throw new InvalidOperationException($"Unknown trigger type '{dto.TriggerType}'.");

        var definition = new WorkflowDefinition
        {
            OrganizationId = orgId,
            Name           = dto.Name,
            Description    = dto.Description,
            TriggerType    = dto.TriggerType,
            IsActive       = true
        };
        _repo.AddDefinition(definition);
        await _repo.SaveChangesAsync();

        var initialGraph =
            "{\"nodes\":[{\"id\":\"trigger-1\",\"type\":\"trigger\",\"x\":300,\"y\":100,\"data\":{\"triggerType\":\"" +
            dto.TriggerType + "\",\"label\":\"" + triggerLabel + "\"}}],\"edges\":[]}";

        var draft = new WorkflowVersion
        {
            WorkflowDefinitionId = definition.Id,
            VersionNumber        = 1,
            GraphJson            = initialGraph,
            Status               = "Draft"
        };
        _repo.AddVersion(draft);
        await _repo.SaveChangesAsync();

        return MapToDto(definition, draft);
    }

    public async Task<WorkflowDefinitionDto> UpdateAsync(int orgId, int id, UpdateWorkflowDefinitionDto dto)
    {
        var definition = await GetOwnedAsync(orgId, id);
        definition.Name        = dto.Name;
        definition.Description = dto.Description;
        definition.IsActive    = dto.IsActive;
        definition.UpdatedDate = DateTime.UtcNow;

        _repo.UpdateDefinition(definition);
        await _repo.SaveChangesAsync();

        var draft = await _repo.GetDraftVersionAsync(definition.Id);
        return MapToDto(definition, draft);
    }

    public async Task DeleteAsync(int orgId, int id)
    {
        var definition = await GetOwnedAsync(orgId, id);
        _repo.RemoveDefinition(definition);
        await _repo.SaveChangesAsync();
    }

    public async Task<List<WorkflowVersionDto>> GetVersionsAsync(int orgId, int id)
    {
        await GetOwnedAsync(orgId, id);
        var versions = await _repo.GetVersionsAsync(id);
        return versions.Select(MapVersionToDto).ToList();
    }

    public async Task<WorkflowVersionDto> GetDraftAsync(int orgId, int id)
    {
        await GetOwnedAsync(orgId, id);
        var draft = await _repo.GetDraftVersionAsync(id)
            ?? throw new InvalidOperationException("This workflow has no draft — it may need to be recreated.");
        return MapVersionToDto(draft);
    }

    public async Task<WorkflowVersionDto> SaveDraftAsync(int orgId, int id, SaveDraftGraphDto dto)
    {
        await GetOwnedAsync(orgId, id);
        var draft = await _repo.GetDraftVersionAsync(id)
            ?? throw new InvalidOperationException("This workflow has no draft — it may need to be recreated.");

        draft.GraphJson  = dto.GraphJson;
        draft.UpdatedDate = DateTime.UtcNow;
        _repo.UpdateVersion(draft);
        await _repo.SaveChangesAsync();
        return MapVersionToDto(draft);
    }

    public async Task<WorkflowDefinitionDto> PublishAsync(int orgId, int id, int userId)
    {
        var definition = await GetOwnedAsync(orgId, id);
        var draft = await _repo.GetDraftVersionAsync(id)
            ?? throw new InvalidOperationException("This workflow has no draft to publish.");

        if (definition.PublishedVersionId.HasValue && definition.PublishedVersionId != draft.Id)
        {
            var previouslyPublished = await _repo.GetVersionAsync(id, definition.PublishedVersionId.Value);
            if (previouslyPublished != null)
            {
                previouslyPublished.Status = "Archived";
                _repo.UpdateVersion(previouslyPublished);
            }
        }

        draft.Status        = "Published";
        draft.PublishedAt   = DateTime.UtcNow;
        draft.PublishedByUserId = userId;
        _repo.UpdateVersion(draft);

        definition.PublishedVersionId = draft.Id;
        definition.UpdatedDate = DateTime.UtcNow;
        _repo.UpdateDefinition(definition);

        // Start a fresh draft (a copy of what was just published) so editing can continue immediately.
        var versions = await _repo.GetVersionsAsync(id);
        var nextVersionNumber = versions.Max(v => v.VersionNumber) + 1;
        var newDraft = new WorkflowVersion
        {
            WorkflowDefinitionId = id,
            VersionNumber        = nextVersionNumber,
            GraphJson            = draft.GraphJson,
            Status               = "Draft"
        };
        _repo.AddVersion(newDraft);

        await _repo.SaveChangesAsync();
        return MapToDto(definition, newDraft);
    }

    public List<TriggerTypeDto> GetTriggerTypes() => new()
    {
        new TriggerTypeDto
        {
            Key = "LeaveRequestSubmitted",
            Label = "New Leave Request Submitted",
            Fields = new List<WorkflowFieldDto>
            {
                new() { Key = "Days", Label = "Number of Days", Type = "number" },
                new() { Key = "IsHalfDay", Label = "Is Half Day", Type = "boolean" },
                new() { Key = "IsPaid", Label = "Leave Type Is Paid", Type = "boolean" },
                new() { Key = "LeaveTypeName", Label = "Leave Type", Type = "string" },
                new() { Key = "DepartmentName", Label = "Department", Type = "string" },
                new() { Key = "DesignationTitle", Label = "Designation", Type = "string" },
                new() { Key = "EmploymentType", Label = "Employment Type", Type = "string",
                        Options = new List<string> { "FullTime", "PartTime", "Contract", "Intern" } }
            }
        }
    };

    public List<ActionTypeDto> GetActionTypes() => new()
    {
        new ActionTypeDto
        {
            Key = "AutoApprove", Label = "Auto Approve",
            ConfigFields = new List<WorkflowFieldDto>()
        },
        new ActionTypeDto
        {
            Key = "AutoReject", Label = "Auto Reject",
            ConfigFields = new List<WorkflowFieldDto>()
        },
        new ActionTypeDto
        {
            Key = "Notify", Label = "Send Notification",
            ConfigFields = new List<WorkflowFieldDto>
            {
                new() { Key = "recipient", Label = "Recipient", Type = "string",
                        Options = new List<string> { "Employee", "Manager", "HR" } },
                new() { Key = "title", Label = "Title", Type = "string" },
                new() { Key = "message", Label = "Message (use {Field} placeholders)", Type = "string" }
            }
        },
        new ActionTypeDto
        {
            Key = "End", Label = "End Workflow",
            ConfigFields = new List<WorkflowFieldDto>()
        }
    };

    public async Task<List<WorkflowExecutionDto>> GetExecutionsAsync(int orgId, int id)
    {
        await GetOwnedAsync(orgId, id);
        var executions = await _repo.GetExecutionsAsync(orgId, id);
        return executions.Select(e => new WorkflowExecutionDto
        {
            Id                = e.Id,
            TriggerEntityType = e.TriggerEntityType,
            TriggerEntityId   = e.TriggerEntityId,
            Status            = e.Status,
            ErrorMessage      = e.ErrorMessage,
            PathJson          = e.PathJson,
            CreatedDate       = e.CreatedDate
        }).ToList();
    }

    private async Task<WorkflowDefinition> GetOwnedAsync(int orgId, int id) =>
        await _repo.GetByIdAsync(orgId, id)
            ?? throw new KeyNotFoundException($"Workflow {id} not found.");

    private WorkflowDefinitionDto MapToDto(WorkflowDefinition d, WorkflowVersion? draft) => new()
    {
        Id                     = d.Id,
        Name                   = d.Name,
        Description            = d.Description,
        TriggerType            = d.TriggerType,
        TriggerLabel           = GetTriggerTypes().FirstOrDefault(t => t.Key == d.TriggerType)?.Label ?? d.TriggerType,
        IsActive               = d.IsActive,
        PublishedVersionId     = d.PublishedVersionId,
        PublishedVersionNumber = d.PublishedVersion?.VersionNumber,
        DraftVersionId         = draft?.Id ?? 0,
        DraftVersionNumber     = draft?.VersionNumber ?? 0,
        PublishedAt            = d.PublishedVersion?.PublishedAt,
        UpdatedDate            = d.UpdatedDate ?? d.CreatedDate
    };

    private static WorkflowVersionDto MapVersionToDto(WorkflowVersion v) => new()
    {
        Id              = v.Id,
        VersionNumber   = v.VersionNumber,
        GraphJson       = v.GraphJson,
        Status          = v.Status,
        PublishedAt     = v.PublishedAt,
        PublishedByName = v.PublishedByUser?.Name,
        CreatedDate     = v.CreatedDate
    };
}
