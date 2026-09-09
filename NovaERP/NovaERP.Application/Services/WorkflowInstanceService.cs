using AutoMapper;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;

namespace NovaERP.Application.Services;

public class WorkflowInstanceService : IWorkflowInstanceService
{
    private readonly IWorkflowInstanceRepository _instanceRepo;
    private readonly IWorkflowDefinitionRepository _definitionRepo;
    private readonly IUserRepository _userRepo;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly INotificationService _notificationService;
    private readonly IAutomationEngine _automationEngine;
    private readonly IMapper _mapper;

    public WorkflowInstanceService(IWorkflowInstanceRepository instanceRepo, IWorkflowDefinitionRepository definitionRepo,
        IUserRepository userRepo, IAuditLogWriter auditLogWriter, INotificationService notificationService,
        IAutomationEngine automationEngine, IMapper mapper)
    {
        _instanceRepo = instanceRepo;
        _definitionRepo = definitionRepo;
        _userRepo = userRepo;
        _auditLogWriter = auditLogWriter;
        _notificationService = notificationService;
        _automationEngine = automationEngine;
        _mapper = mapper;
    }

    public async Task<WorkflowInstanceDto> StartAsync(int orgId, int submittedByUserId, StartWorkflowDto dto)
    {
        var definition = await _definitionRepo.GetActiveForEntityTypeAsync(orgId, dto.EntityType)
            ?? throw new InvalidOperationException($"No approval workflow configured for entity type '{dto.EntityType}'");

        var applicableSteps = definition.Steps
            .Where(s => s.MinAmount == null || (dto.Amount ?? 0m) >= s.MinAmount.Value)
            .OrderBy(s => s.StepOrder)
            .ToList();

        if (applicableSteps.Count == 0)
            throw new InvalidOperationException(
                $"Workflow '{definition.Name}' has no steps applicable to the given amount.");

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definition.Id,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Amount = dto.Amount,
            Status = "Pending",
            CurrentStepOrder = applicableSteps[0].StepOrder,
            SubmittedByUserId = submittedByUserId,
            SubmittedDate = DateTime.UtcNow,
            Steps = applicableSteps.Select(s => new WorkflowStepInstance
            {
                StepOrder = s.StepOrder,
                ApproverRoleId = s.ApproverRoleId,
                Status = "Pending"
            }).ToList()
        };

        _instanceRepo.Add(instance);
        await _instanceRepo.SaveChangesAsync();

        return await ToDtoAsync(instance, definition.Name);
    }

    public async Task<WorkflowInstanceDto> ApproveAsync(int orgId, int instanceId, int actingUserId, WorkflowActionDto dto)
    {
        var (instance, currentStep, actingUser) = await LoadAndAuthorizeAsync(orgId, instanceId, actingUserId);

        currentStep.Status = "Approved";
        currentStep.ApproverUserId = actingUserId;
        currentStep.ActionedDate = DateTime.UtcNow;
        currentStep.Comments = dto.Comments;

        var remainingSteps = instance.Steps
            .Where(s => s.StepOrder > currentStep.StepOrder)
            .OrderBy(s => s.StepOrder)
            .ToList();

        if (remainingSteps.Count == 0)
        {
            instance.Status = "Approved";
            instance.CompletedDate = DateTime.UtcNow;

            await _auditLogWriter.WriteAsync(orgId, actingUserId, actingUser?.Name, "POST",
                $"/api/workflow-instances/{instanceId}/approve", "Workflow Fully Approved", 200, null, 0);

            await _notificationService.CreateAsync(orgId, new CreateNotificationDto
            {
                UserId = instance.SubmittedByUserId,
                Title = "Approval request approved",
                Message = $"Your {instance.EntityType} #{instance.EntityId} request was approved.",
                Type = "Success"
            });

            await _automationEngine.HandleEventAsync(orgId, AutomationEvents.WorkflowApproved, new Dictionary<string, string>
            {
                ["EntityType"] = instance.EntityType,
                ["EntityId"] = instance.EntityId.ToString(),
                ["Amount"] = instance.Amount?.ToString() ?? string.Empty
            });
        }
        else
        {
            instance.CurrentStepOrder = remainingSteps[0].StepOrder;
        }

        _instanceRepo.Update(instance);
        await _instanceRepo.SaveChangesAsync();

        var definitionName = instance.WorkflowDefinition?.Name ?? string.Empty;
        return await ToDtoAsync(instance, definitionName);
    }

    public async Task<WorkflowInstanceDto> RejectAsync(int orgId, int instanceId, int actingUserId, WorkflowActionDto dto)
    {
        var (instance, currentStep, actingUser) = await LoadAndAuthorizeAsync(orgId, instanceId, actingUserId);

        currentStep.Status = "Rejected";
        currentStep.ApproverUserId = actingUserId;
        currentStep.ActionedDate = DateTime.UtcNow;
        currentStep.Comments = dto.Comments;

        instance.Status = "Rejected";
        instance.CompletedDate = DateTime.UtcNow;

        await _auditLogWriter.WriteAsync(orgId, actingUserId, actingUser?.Name, "POST",
            $"/api/workflow-instances/{instanceId}/reject", "Workflow Rejected", 200, null, 0);

        await _notificationService.CreateAsync(orgId, new CreateNotificationDto
        {
            UserId = instance.SubmittedByUserId,
            Title = "Approval request rejected",
            Message = $"Your {instance.EntityType} #{instance.EntityId} request was rejected.",
            Type = "Warning"
        });

        await _automationEngine.HandleEventAsync(orgId, AutomationEvents.WorkflowRejected, new Dictionary<string, string>
        {
            ["EntityType"] = instance.EntityType,
            ["EntityId"] = instance.EntityId.ToString(),
            ["Amount"] = instance.Amount?.ToString() ?? string.Empty
        });

        _instanceRepo.Update(instance);
        await _instanceRepo.SaveChangesAsync();

        var definitionName = instance.WorkflowDefinition?.Name ?? string.Empty;
        return await ToDtoAsync(instance, definitionName);
    }

    public async Task<List<WorkflowInstanceDto>> GetPendingForUserAsync(int orgId, int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found");

        var instances = await _instanceRepo.GetPendingForRoleAsync(orgId, user.RoleId);
        var result = new List<WorkflowInstanceDto>();
        foreach (var instance in instances)
            result.Add(await ToDtoAsync(instance, instance.WorkflowDefinition?.Name ?? string.Empty));
        return result;
    }

    public async Task<List<WorkflowInstanceDto>> GetByEntityAsync(int orgId, string entityType, int entityId)
    {
        var instances = await _instanceRepo.GetByEntityAsync(orgId, entityType, entityId);
        var result = new List<WorkflowInstanceDto>();
        foreach (var instance in instances)
            result.Add(await ToDtoAsync(instance, instance.WorkflowDefinition?.Name ?? string.Empty));
        return result;
    }

    private async Task<(WorkflowInstance instance, WorkflowStepInstance currentStep, User? actingUser)> LoadAndAuthorizeAsync(
        int orgId, int instanceId, int actingUserId)
    {
        var instance = await _instanceRepo.GetByIdAsync(instanceId)
            ?? throw new KeyNotFoundException($"WorkflowInstance {instanceId} not found");

        if (instance.Status != "Pending")
            throw new InvalidOperationException($"WorkflowInstance {instanceId} is not pending (status: {instance.Status})");

        var currentStep = instance.Steps.FirstOrDefault(s => s.StepOrder == instance.CurrentStepOrder && s.Status == "Pending")
            ?? throw new InvalidOperationException($"WorkflowInstance {instanceId} has no pending current step");

        var actingUser = await _userRepo.GetByIdAsync(actingUserId)
            ?? throw new KeyNotFoundException($"User {actingUserId} not found");

        if (currentStep.ApproverRoleId != null && currentStep.ApproverRoleId != actingUser.RoleId)
            throw new UnauthorizedAccessException(
                "The acting user does not hold the required approver role for the current workflow step.");

        return (instance, currentStep, actingUser);
    }

    private Task<WorkflowInstanceDto> ToDtoAsync(WorkflowInstance instance, string definitionName)
    {
        return Task.FromResult(new WorkflowInstanceDto
        {
            Id = instance.Id,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowDefinitionName = definitionName,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            Amount = instance.Amount,
            Status = instance.Status,
            CurrentStepOrder = instance.CurrentStepOrder,
            SubmittedByUserId = instance.SubmittedByUserId,
            SubmittedDate = instance.SubmittedDate,
            CompletedDate = instance.CompletedDate,
            Steps = instance.Steps.OrderBy(s => s.StepOrder).Select(s => new WorkflowStepInstanceDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                ApproverRoleId = s.ApproverRoleId,
                ApproverRoleName = s.ApproverRole?.Name,
                ApproverUserId = s.ApproverUserId,
                ApproverUserName = s.ApproverUser?.Name,
                Status = s.Status,
                ActionedDate = s.ActionedDate,
                Comments = s.Comments
            }).ToList()
        });
    }
}
