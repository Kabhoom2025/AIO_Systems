using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.WorkItems;

/// <summary>Shared helper so every mutating WorkItem handler records one WorkItemActivity row,
/// giving the frontend a real activity timeline instead of a fake one.</summary>
internal static class WorkItemActivityRecorder
{
    public static void Record(IProjectFlowDbContext db, Guid workItemId, Guid userId, string action,
        string? fieldName = null, string? oldValue = null, string? newValue = null)
    {
        db.WorkItemActivities.Add(new WorkItemActivity
        {
            WorkItemId = workItemId,
            UserId = userId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
}

// ---------------------------------------------------------------------------
// CreateWorkItem
// ---------------------------------------------------------------------------

public record CreateWorkItemCommand(Guid ProjectId, Guid? ParentWorkItemId, string Title, string? Description,
    WorkItemPriority Priority, WorkItemType Type, int? StoryPoints, decimal? EstimatedHours,
    Guid? AssigneeUserId, Guid ReporterUserId, DateTime? DueDate, bool IsRecurring,
    int? RecurrenceIntervalDays) : IRequest<WorkItemDto>;

public class CreateWorkItemCommandValidator : AbstractValidator<CreateWorkItemCommand>
{
    public CreateWorkItemCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ReporterUserId).NotEmpty();
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.StoryPoints).GreaterThanOrEqualTo(0).When(x => x.StoryPoints.HasValue);
        RuleFor(x => x.EstimatedHours).GreaterThanOrEqualTo(0).When(x => x.EstimatedHours.HasValue);
        RuleFor(x => x.RecurrenceIntervalDays).GreaterThan(0)
            .When(x => x.IsRecurring)
            .WithMessage("RecurrenceIntervalDays must be > 0 when IsRecurring is true.");
        RuleFor(x => x.RecurrenceIntervalDays).Null()
            .When(x => !x.IsRecurring)
            .WithMessage("RecurrenceIntervalDays must be null unless IsRecurring is true.");
    }
}

public class CreateWorkItemCommandHandler : IRequestHandler<CreateWorkItemCommand, WorkItemDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly IWorkflowEngine _workflowEngine;

    public CreateWorkItemCommandHandler(IProjectFlowDbContext db, IMapper mapper, INotificationDispatcher notifications, IWorkflowEngine workflowEngine)
    {
        _db = db; _mapper = mapper; _notifications = notifications; _workflowEngine = workflowEngine;
    }

    public async Task<WorkItemDto> Handle(CreateWorkItemCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.ReporterUserId, cancellationToken))
            throw new NotFoundException("User", request.ReporterUserId);
        if (request.AssigneeUserId.HasValue && !await _db.Users.AnyAsync(u => u.Id == request.AssigneeUserId.Value, cancellationToken))
            throw new NotFoundException("User", request.AssigneeUserId.Value);
        if (request.ParentWorkItemId.HasValue && !await _db.WorkItems.AnyAsync(w => w.Id == request.ParentWorkItemId.Value, cancellationToken))
            throw new NotFoundException("WorkItem", request.ParentWorkItemId.Value);

        // New items always land at the bottom of the Backlog column; sparse position so later
        // drag-drop reorders never need to renumber every sibling.
        var maxPosition = await _db.WorkItems
            .Where(w => w.ProjectId == request.ProjectId && w.Status == WorkItemStatus.Backlog)
            .Select(w => (double?)w.Position).MaxAsync(cancellationToken) ?? 0d;

        var workItem = new WorkItem
        {
            ProjectId = request.ProjectId,
            ParentWorkItemId = request.ParentWorkItemId,
            Title = request.Title,
            Description = request.Description,
            Status = WorkItemStatus.Backlog,
            Priority = request.Priority,
            Type = request.Type,
            StoryPoints = request.StoryPoints,
            EstimatedHours = request.EstimatedHours,
            AssigneeUserId = request.AssigneeUserId,
            ReporterUserId = request.ReporterUserId,
            DueDate = request.DueDate,
            IsRecurring = request.IsRecurring,
            RecurrenceIntervalDays = request.IsRecurring ? request.RecurrenceIntervalDays : null,
            Position = maxPosition + 1024d
        };
        _db.WorkItems.Add(workItem);
        WorkItemActivityRecorder.Record(_db, workItem.Id, request.ReporterUserId, "Created");
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkItems.Include(w => w.AssigneeUser).Include(w => w.ReporterUser)
            .Include(w => w.WorkItemLabels).ThenInclude(wl => wl.Label)
            .FirstAsync(w => w.Id == workItem.Id, cancellationToken);

        if (request.AssigneeUserId.HasValue)
            await _notifications.DispatchAsync(request.AssigneeUserId.Value, NotificationType.Assignment,
                "You were assigned a task", $"You were assigned \"{workItem.Title}\".",
                $"/projects/{workItem.ProjectId}/tasks/{workItem.Id}", cancellationToken);

        await _workflowEngine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemCreated, workItem.ProjectId, withNav, cancellationToken);
        if (request.AssigneeUserId.HasValue)
            await _workflowEngine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemAssigned, workItem.ProjectId, withNav, cancellationToken);

        return _mapper.Map<WorkItemDto>(withNav);
    }
}

// ---------------------------------------------------------------------------
// UpdateWorkItem
// ---------------------------------------------------------------------------

public record UpdateWorkItemCommand(Guid Id, string Title, string? Description, WorkItemPriority Priority,
    WorkItemType Type, int? StoryPoints, decimal? EstimatedHours, Guid? AssigneeUserId, DateTime? DueDate,
    bool IsRecurring, int? RecurrenceIntervalDays, Guid ActorUserId) : IRequest<WorkItemDto>;

public class UpdateWorkItemCommandValidator : AbstractValidator<UpdateWorkItemCommand>
{
    public UpdateWorkItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ActorUserId).NotEmpty();
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.RecurrenceIntervalDays).GreaterThan(0).When(x => x.IsRecurring);
        RuleFor(x => x.RecurrenceIntervalDays).Null().When(x => !x.IsRecurring);
    }
}

public class UpdateWorkItemCommandHandler : IRequestHandler<UpdateWorkItemCommand, WorkItemDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly IWorkflowEngine _workflowEngine;

    public UpdateWorkItemCommandHandler(IProjectFlowDbContext db, IMapper mapper, INotificationDispatcher notifications, IWorkflowEngine workflowEngine)
    {
        _db = db; _mapper = mapper; _notifications = notifications; _workflowEngine = workflowEngine;
    }

    public async Task<WorkItemDto> Handle(UpdateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.Id);
        if (request.AssigneeUserId.HasValue && !await _db.Users.AnyAsync(u => u.Id == request.AssigneeUserId.Value, cancellationToken))
            throw new NotFoundException("User", request.AssigneeUserId.Value);

        var assigneeChangedTo = request.AssigneeUserId.HasValue && workItem.AssigneeUserId != request.AssigneeUserId ? request.AssigneeUserId : null;

        if (workItem.Priority != request.Priority)
            WorkItemActivityRecorder.Record(_db, workItem.Id, request.ActorUserId, "PriorityChanged", "Priority", workItem.Priority.ToString(), request.Priority.ToString());
        if (workItem.AssigneeUserId != request.AssigneeUserId)
            WorkItemActivityRecorder.Record(_db, workItem.Id, request.ActorUserId, "Assigned", "AssigneeUserId", workItem.AssigneeUserId?.ToString(), request.AssigneeUserId?.ToString());

        workItem.Title = request.Title;
        workItem.Description = request.Description;
        workItem.Priority = request.Priority;
        workItem.Type = request.Type;
        workItem.StoryPoints = request.StoryPoints;
        workItem.EstimatedHours = request.EstimatedHours;
        workItem.AssigneeUserId = request.AssigneeUserId;
        workItem.DueDate = request.DueDate;
        workItem.IsRecurring = request.IsRecurring;
        workItem.RecurrenceIntervalDays = request.IsRecurring ? request.RecurrenceIntervalDays : null;
        workItem.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkItems.Include(w => w.AssigneeUser).Include(w => w.ReporterUser)
            .Include(w => w.WorkItemLabels).ThenInclude(wl => wl.Label)
            .FirstAsync(w => w.Id == workItem.Id, cancellationToken);

        if (assigneeChangedTo.HasValue)
            await _notifications.DispatchAsync(assigneeChangedTo.Value, NotificationType.Assignment,
                "You were assigned a task", $"You were assigned \"{workItem.Title}\".",
                $"/projects/{workItem.ProjectId}/tasks/{workItem.Id}", cancellationToken);

        if (assigneeChangedTo.HasValue)
            await _workflowEngine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemAssigned, workItem.ProjectId, withNav, cancellationToken);

        return _mapper.Map<WorkItemDto>(withNav);
    }
}

// ---------------------------------------------------------------------------
// MoveWorkItem (Kanban drag-drop) — the recurring-task auto-creation side effect lives here.
// ---------------------------------------------------------------------------

public record MoveWorkItemCommand(Guid Id, WorkItemStatus NewStatus, double NewPosition, Guid ActorUserId) : IRequest<WorkItemDto>;

public class MoveWorkItemCommandValidator : AbstractValidator<MoveWorkItemCommand>
{
    public MoveWorkItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
        RuleFor(x => x.NewPosition).Must(double.IsFinite).WithMessage("NewPosition must be a finite number.");
    }
}

public class MoveWorkItemCommandHandler : IRequestHandler<MoveWorkItemCommand, WorkItemDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWorkflowEngine _workflowEngine;

    public MoveWorkItemCommandHandler(IProjectFlowDbContext db, IMapper mapper, IWorkflowEngine workflowEngine)
    {
        _db = db; _mapper = mapper; _workflowEngine = workflowEngine;
    }

    public async Task<WorkItemDto> Handle(MoveWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.Id);

        var oldStatus = workItem.Status;
        if (oldStatus != request.NewStatus)
            WorkItemActivityRecorder.Record(_db, workItem.Id, request.ActorUserId, "StatusChanged", "Status", oldStatus.ToString(), request.NewStatus.ToString());

        workItem.Status = request.NewStatus;
        workItem.Position = request.NewPosition;
        workItem.UpdatedAt = DateTime.UtcNow;

        // Recurring-task side effect: moving a recurring item into Done immediately spins up the
        // next occurrence (simple "every N days" clone) rather than a background job — this is a
        // small synchronous side effect, not something that warrants Hangfire.
        if (request.NewStatus == WorkItemStatus.Done && oldStatus != WorkItemStatus.Done
            && workItem.IsRecurring && workItem.RecurrenceIntervalDays.HasValue)
        {
            var nextDueDate = (workItem.DueDate ?? DateTime.UtcNow).AddDays(workItem.RecurrenceIntervalDays.Value);
            var backlogMax = await _db.WorkItems
                .Where(w => w.ProjectId == workItem.ProjectId && w.Status == WorkItemStatus.Backlog)
                .Select(w => (double?)w.Position).MaxAsync(cancellationToken) ?? 0d;

            var nextOccurrence = new WorkItem
            {
                ProjectId = workItem.ProjectId,
                ParentWorkItemId = workItem.ParentWorkItemId,
                Title = workItem.Title,
                Description = workItem.Description,
                Status = WorkItemStatus.Backlog,
                Priority = workItem.Priority,
                Type = workItem.Type,
                StoryPoints = workItem.StoryPoints,
                EstimatedHours = workItem.EstimatedHours,
                AssigneeUserId = workItem.AssigneeUserId,
                ReporterUserId = workItem.ReporterUserId,
                DueDate = nextDueDate,
                IsRecurring = true,
                RecurrenceIntervalDays = workItem.RecurrenceIntervalDays,
                Position = backlogMax + 1024d
            };
            _db.WorkItems.Add(nextOccurrence);
            WorkItemActivityRecorder.Record(_db, nextOccurrence.Id, request.ActorUserId, "Created", "RecurrenceOf", workItem.Id.ToString(), null);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkItems.Include(w => w.AssigneeUser).Include(w => w.ReporterUser)
            .Include(w => w.WorkItemLabels).ThenInclude(wl => wl.Label)
            .FirstAsync(w => w.Id == workItem.Id, cancellationToken);

        if (oldStatus != request.NewStatus)
            await _workflowEngine.EvaluateTriggerAsync(WorkflowTriggerType.WorkItemStatusChanged, workItem.ProjectId, withNav, cancellationToken);

        return _mapper.Map<WorkItemDto>(withNav);
    }
}

// ---------------------------------------------------------------------------
// DeleteWorkItem
// ---------------------------------------------------------------------------

public record DeleteWorkItemCommand(Guid Id) : IRequest<Unit>;

public class DeleteWorkItemCommandValidator : AbstractValidator<DeleteWorkItemCommand>
{
    public DeleteWorkItemCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteWorkItemCommandHandler : IRequestHandler<DeleteWorkItemCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteWorkItemCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.Id);

        _db.WorkItems.Remove(workItem);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Checklist
// ---------------------------------------------------------------------------

public record AddChecklistItemCommand(Guid WorkItemId, string Text) : IRequest<ChecklistItemDto>;

public class AddChecklistItemCommandValidator : AbstractValidator<AddChecklistItemCommand>
{
    public AddChecklistItemCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
    }
}

public class AddChecklistItemCommandHandler : IRequestHandler<AddChecklistItemCommand, ChecklistItemDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddChecklistItemCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ChecklistItemDto> Handle(AddChecklistItemCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);

        var maxPosition = await _db.ChecklistItems.Where(c => c.WorkItemId == request.WorkItemId)
            .Select(c => (int?)c.Position).MaxAsync(cancellationToken) ?? -1;

        var item = new ChecklistItem { WorkItemId = request.WorkItemId, Text = request.Text, Position = maxPosition + 1 };
        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ChecklistItemDto>(item);
    }
}

public record ToggleChecklistItemCommand(Guid WorkItemId, Guid ItemId) : IRequest<ChecklistItemDto>;

public class ToggleChecklistItemCommandValidator : AbstractValidator<ToggleChecklistItemCommand>
{
    public ToggleChecklistItemCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}

public class ToggleChecklistItemCommandHandler : IRequestHandler<ToggleChecklistItemCommand, ChecklistItemDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ToggleChecklistItemCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ChecklistItemDto> Handle(ToggleChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _db.ChecklistItems.FirstOrDefaultAsync(
            c => c.Id == request.ItemId && c.WorkItemId == request.WorkItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        item.IsDone = !item.IsDone;
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ChecklistItemDto>(item);
    }
}

public record DeleteChecklistItemCommand(Guid WorkItemId, Guid ItemId) : IRequest<Unit>;

public class DeleteChecklistItemCommandValidator : AbstractValidator<DeleteChecklistItemCommand>
{
    public DeleteChecklistItemCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}

public class DeleteChecklistItemCommandHandler : IRequestHandler<DeleteChecklistItemCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteChecklistItemCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _db.ChecklistItems.FirstOrDefaultAsync(
            c => c.Id == request.ItemId && c.WorkItemId == request.WorkItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        _db.ChecklistItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Labels on a WorkItem
// ---------------------------------------------------------------------------

public record AddWorkItemLabelCommand(Guid WorkItemId, Guid LabelId) : IRequest<Unit>;

public class AddWorkItemLabelCommandValidator : AbstractValidator<AddWorkItemLabelCommand>
{
    public AddWorkItemLabelCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.LabelId).NotEmpty();
    }
}

public class AddWorkItemLabelCommandHandler : IRequestHandler<AddWorkItemLabelCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public AddWorkItemLabelCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(AddWorkItemLabelCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);
        if (!await _db.Labels.AnyAsync(l => l.Id == request.LabelId, cancellationToken))
            throw new NotFoundException("Label", request.LabelId);

        var exists = await _db.WorkItemLabels.AnyAsync(
            wl => wl.WorkItemId == request.WorkItemId && wl.LabelId == request.LabelId, cancellationToken);
        if (!exists)
        {
            _db.WorkItemLabels.Add(new WorkItemLabel { WorkItemId = request.WorkItemId, LabelId = request.LabelId });
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

public record RemoveWorkItemLabelCommand(Guid WorkItemId, Guid LabelId) : IRequest<Unit>;

public class RemoveWorkItemLabelCommandValidator : AbstractValidator<RemoveWorkItemLabelCommand>
{
    public RemoveWorkItemLabelCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.LabelId).NotEmpty();
    }
}

public class RemoveWorkItemLabelCommandHandler : IRequestHandler<RemoveWorkItemLabelCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RemoveWorkItemLabelCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveWorkItemLabelCommand request, CancellationToken cancellationToken)
    {
        var link = await _db.WorkItemLabels.FirstOrDefaultAsync(
            wl => wl.WorkItemId == request.WorkItemId && wl.LabelId == request.LabelId, cancellationToken);
        if (link != null)
        {
            _db.WorkItemLabels.Remove(link);
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Followers
// ---------------------------------------------------------------------------

public record AddWorkItemFollowerCommand(Guid WorkItemId, Guid UserId) : IRequest<Unit>;

public class AddWorkItemFollowerCommandValidator : AbstractValidator<AddWorkItemFollowerCommand>
{
    public AddWorkItemFollowerCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class AddWorkItemFollowerCommandHandler : IRequestHandler<AddWorkItemFollowerCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public AddWorkItemFollowerCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(AddWorkItemFollowerCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User", request.UserId);

        var exists = await _db.WorkItemFollowers.AnyAsync(
            f => f.WorkItemId == request.WorkItemId && f.UserId == request.UserId, cancellationToken);
        if (!exists)
        {
            _db.WorkItemFollowers.Add(new WorkItemFollower { WorkItemId = request.WorkItemId, UserId = request.UserId });
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

public record RemoveWorkItemFollowerCommand(Guid WorkItemId, Guid UserId) : IRequest<Unit>;

public class RemoveWorkItemFollowerCommandValidator : AbstractValidator<RemoveWorkItemFollowerCommand>
{
    public RemoveWorkItemFollowerCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class RemoveWorkItemFollowerCommandHandler : IRequestHandler<RemoveWorkItemFollowerCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RemoveWorkItemFollowerCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveWorkItemFollowerCommand request, CancellationToken cancellationToken)
    {
        var follower = await _db.WorkItemFollowers.FirstOrDefaultAsync(
            f => f.WorkItemId == request.WorkItemId && f.UserId == request.UserId, cancellationToken);
        if (follower != null)
        {
            _db.WorkItemFollowers.Remove(follower);
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Dependencies
// ---------------------------------------------------------------------------

public record AddWorkItemDependencyCommand(Guid WorkItemId, Guid DependsOnWorkItemId, WorkItemDependencyType DependencyType) : IRequest<WorkItemDependencyDto>;

public class AddWorkItemDependencyCommandValidator : AbstractValidator<AddWorkItemDependencyCommand>
{
    public AddWorkItemDependencyCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.DependsOnWorkItemId).NotEmpty();
        RuleFor(x => x.DependencyType).IsInEnum();
        RuleFor(x => x).Must(x => x.WorkItemId != x.DependsOnWorkItemId)
            .WithMessage("A work item cannot depend on itself.");
    }
}

public class AddWorkItemDependencyCommandHandler : IRequestHandler<AddWorkItemDependencyCommand, WorkItemDependencyDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddWorkItemDependencyCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemDependencyDto> Handle(AddWorkItemDependencyCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.DependsOnWorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.DependsOnWorkItemId);
        if (await _db.WorkItemDependencies.AnyAsync(
            d => d.WorkItemId == request.WorkItemId && d.DependsOnWorkItemId == request.DependsOnWorkItemId, cancellationToken))
            throw new ConflictException("This dependency already exists.");

        var dependency = new WorkItemDependency
        {
            WorkItemId = request.WorkItemId,
            DependsOnWorkItemId = request.DependsOnWorkItemId,
            DependencyType = request.DependencyType
        };
        _db.WorkItemDependencies.Add(dependency);
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkItemDependencies.Include(d => d.DependsOnWorkItem)
            .FirstAsync(d => d.Id == dependency.Id, cancellationToken);
        return _mapper.Map<WorkItemDependencyDto>(withNav);
    }
}

public record RemoveWorkItemDependencyCommand(Guid Id) : IRequest<Unit>;

public class RemoveWorkItemDependencyCommandValidator : AbstractValidator<RemoveWorkItemDependencyCommand>
{
    public RemoveWorkItemDependencyCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class RemoveWorkItemDependencyCommandHandler : IRequestHandler<RemoveWorkItemDependencyCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RemoveWorkItemDependencyCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveWorkItemDependencyCommand request, CancellationToken cancellationToken)
    {
        var dependency = await _db.WorkItemDependencies.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemDependency", request.Id);

        _db.WorkItemDependencies.Remove(dependency);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Comments
// ---------------------------------------------------------------------------

public record AddWorkItemCommentCommand(Guid WorkItemId, Guid AuthorUserId, string Body, IReadOnlyList<Guid> MentionedUserIds) : IRequest<WorkItemCommentDto>;

public class AddWorkItemCommentCommandValidator : AbstractValidator<AddWorkItemCommentCommand>
{
    public AddWorkItemCommentCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.AuthorUserId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
        RuleFor(x => x.MentionedUserIds).NotNull();
    }
}

public class AddWorkItemCommentCommandHandler : IRequestHandler<AddWorkItemCommentCommand, WorkItemCommentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;

    public AddWorkItemCommentCommandHandler(IProjectFlowDbContext db, IMapper mapper, INotificationDispatcher notifications)
    {
        _db = db; _mapper = mapper; _notifications = notifications;
    }

    public async Task<WorkItemCommentDto> Handle(AddWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == request.WorkItemId, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.WorkItemId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.AuthorUserId, cancellationToken))
            throw new NotFoundException("User", request.AuthorUserId);

        var comment = new WorkItemComment { WorkItemId = request.WorkItemId, AuthorUserId = request.AuthorUserId, Body = request.Body };
        _db.WorkItemComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        var distinctMentions = request.MentionedUserIds.Distinct().ToList();
        foreach (var mentionedUserId in distinctMentions)
            _db.WorkItemCommentMentions.Add(new WorkItemCommentMention { CommentId = comment.Id, MentionedUserId = mentionedUserId });

        WorkItemActivityRecorder.Record(_db, request.WorkItemId, request.AuthorUserId, "Commented");
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var mentionedUserId in distinctMentions)
            await _notifications.DispatchAsync(mentionedUserId, NotificationType.Mention,
                "You were mentioned in a comment", $"You were mentioned in a comment on \"{workItem.Title}\".",
                $"/projects/{workItem.ProjectId}/tasks/{workItem.Id}", cancellationToken);

        var withNav = await _db.WorkItemComments.Include(c => c.AuthorUser).Include(c => c.Mentions)
            .FirstAsync(c => c.Id == comment.Id, cancellationToken);
        return _mapper.Map<WorkItemCommentDto>(withNav);
    }
}

public record UpdateWorkItemCommentCommand(Guid Id, string Body) : IRequest<WorkItemCommentDto>;

public class UpdateWorkItemCommentCommandValidator : AbstractValidator<UpdateWorkItemCommentCommand>
{
    public UpdateWorkItemCommentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}

public class UpdateWorkItemCommentCommandHandler : IRequestHandler<UpdateWorkItemCommentCommand, WorkItemCommentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateWorkItemCommentCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemCommentDto> Handle(UpdateWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _db.WorkItemComments.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemComment", request.Id);

        comment.Body = request.Body;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkItemComments.Include(c => c.AuthorUser).Include(c => c.Mentions)
            .FirstAsync(c => c.Id == comment.Id, cancellationToken);
        return _mapper.Map<WorkItemCommentDto>(withNav);
    }
}

/// <summary>Deletable by its own author, or by an org admin/super admin — deliberately not a
/// full per-comment ACL system, matching the spec's "don't overengineer" guidance.</summary>
public record DeleteWorkItemCommentCommand(Guid Id, Guid ActorUserId) : IRequest<Unit>;

public class DeleteWorkItemCommentCommandValidator : AbstractValidator<DeleteWorkItemCommentCommand>
{
    public DeleteWorkItemCommentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class DeleteWorkItemCommentCommandHandler : IRequestHandler<DeleteWorkItemCommentCommand, Unit>
{
    private static readonly string[] AdminRoleNames = { "OrganizationAdmin", "SuperAdmin" };

    private readonly IProjectFlowDbContext _db;

    public DeleteWorkItemCommentCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteWorkItemCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _db.WorkItemComments.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemComment", request.Id);

        if (comment.AuthorUserId != request.ActorUserId)
        {
            var isAdmin = await _db.UserRoles.Where(ur => ur.UserId == request.ActorUserId)
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .AnyAsync(name => AdminRoleNames.Contains(name), cancellationToken);
            if (!isAdmin)
                throw new UnauthorizedDomainException("Only the comment's author or an organization admin can delete it.");
        }

        _db.WorkItemComments.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Attachments
// ---------------------------------------------------------------------------

public record AddWorkItemAttachmentCommand(Guid WorkItemId, string FileName, string FileUrl, long FileSizeBytes, Guid UploadedByUserId) : IRequest<WorkItemAttachmentDto>;

public class AddWorkItemAttachmentCommandValidator : AbstractValidator<AddWorkItemAttachmentCommand>
{
    public AddWorkItemAttachmentCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.FileUrl).NotEmpty();
        RuleFor(x => x.FileSizeBytes).GreaterThan(0);
        RuleFor(x => x.UploadedByUserId).NotEmpty();
    }
}

public class AddWorkItemAttachmentCommandHandler : IRequestHandler<AddWorkItemAttachmentCommand, WorkItemAttachmentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddWorkItemAttachmentCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemAttachmentDto> Handle(AddWorkItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);

        var attachment = new WorkItemAttachment
        {
            WorkItemId = request.WorkItemId,
            FileName = request.FileName,
            FileUrl = request.FileUrl,
            FileSizeBytes = request.FileSizeBytes,
            UploadedByUserId = request.UploadedByUserId
        };
        _db.WorkItemAttachments.Add(attachment);
        WorkItemActivityRecorder.Record(_db, request.WorkItemId, request.UploadedByUserId, "AttachmentAdded", "FileName", null, request.FileName);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkItemAttachmentDto>(attachment);
    }
}

public record DeleteWorkItemAttachmentCommand(Guid Id) : IRequest<string>;

public class DeleteWorkItemAttachmentCommandValidator : AbstractValidator<DeleteWorkItemAttachmentCommand>
{
    public DeleteWorkItemAttachmentCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

/// <summary>Returns the stored FileUrl (relative path) so the API layer can delete the physical
/// file via IFileStorageService — Application stays transport/storage-agnostic.</summary>
public class DeleteWorkItemAttachmentCommandHandler : IRequestHandler<DeleteWorkItemAttachmentCommand, string>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteWorkItemAttachmentCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<string> Handle(DeleteWorkItemAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _db.WorkItemAttachments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemAttachment", request.Id);

        _db.WorkItemAttachments.Remove(attachment);
        await _db.SaveChangesAsync(cancellationToken);
        return attachment.FileUrl;
    }
}

// ---------------------------------------------------------------------------
// Time logs
// ---------------------------------------------------------------------------

public record LogWorkItemTimeCommand(Guid WorkItemId, Guid UserId, int Minutes, string? Note, DateOnly LoggedDate, bool IsBillable = true) : IRequest<WorkItemTimeLogDto>;

public class LogWorkItemTimeCommandValidator : AbstractValidator<LogWorkItemTimeCommand>
{
    public LogWorkItemTimeCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Minutes).GreaterThan(0);
    }
}

public class LogWorkItemTimeCommandHandler : IRequestHandler<LogWorkItemTimeCommand, WorkItemTimeLogDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public LogWorkItemTimeCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemTimeLogDto> Handle(LogWorkItemTimeCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User", request.UserId);

        var timeLog = new WorkItemTimeLog
        {
            WorkItemId = request.WorkItemId,
            UserId = request.UserId,
            Minutes = request.Minutes,
            Note = request.Note,
            LoggedDate = request.LoggedDate,
            IsBillable = request.IsBillable
        };
        _db.WorkItemTimeLogs.Add(timeLog);
        WorkItemActivityRecorder.Record(_db, request.WorkItemId, request.UserId, "TimeLogged", "Minutes", null, request.Minutes.ToString());
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkItemTimeLogDto>(timeLog);
    }
}

public record SetTimeLogBillableCommand(Guid Id, bool IsBillable) : IRequest<WorkItemTimeLogDto>;

public class SetTimeLogBillableCommandValidator : AbstractValidator<SetTimeLogBillableCommand>
{
    public SetTimeLogBillableCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class SetTimeLogBillableCommandHandler : IRequestHandler<SetTimeLogBillableCommand, WorkItemTimeLogDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public SetTimeLogBillableCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemTimeLogDto> Handle(SetTimeLogBillableCommand request, CancellationToken cancellationToken)
    {
        var timeLog = await _db.WorkItemTimeLogs.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemTimeLog", request.Id);

        timeLog.IsBillable = request.IsBillable;
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkItemTimeLogDto>(timeLog);
    }
}

// ---------------------------------------------------------------------------
// Sprint assignment (Scrum board drag-drop from backlog into a sprint, or back out)
// ---------------------------------------------------------------------------

public record MoveWorkItemToSprintCommand(Guid Id, Guid? SprintId) : IRequest<WorkItemDto>;

public class MoveWorkItemToSprintCommandValidator : AbstractValidator<MoveWorkItemToSprintCommand>
{
    public MoveWorkItemToSprintCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class MoveWorkItemToSprintCommandHandler : IRequestHandler<MoveWorkItemToSprintCommand, WorkItemDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public MoveWorkItemToSprintCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemDto> Handle(MoveWorkItemToSprintCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _db.WorkItems.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.Id);

        if (request.SprintId.HasValue)
        {
            var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == request.SprintId.Value, cancellationToken)
                ?? throw new NotFoundException("Sprint", request.SprintId.Value);
            if (sprint.ProjectId != workItem.ProjectId)
                throw new DomainException("Cannot assign a work item to a sprint belonging to a different project.");
        }

        workItem.SprintId = request.SprintId;
        workItem.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkItems.Include(w => w.AssigneeUser).Include(w => w.ReporterUser)
            .Include(w => w.WorkItemLabels).ThenInclude(wl => wl.Label)
            .FirstAsync(w => w.Id == workItem.Id, cancellationToken);
        return _mapper.Map<WorkItemDto>(withNav);
    }
}

public record DeleteWorkItemTimeLogCommand(Guid Id) : IRequest<Unit>;

public class DeleteWorkItemTimeLogCommandValidator : AbstractValidator<DeleteWorkItemTimeLogCommand>
{
    public DeleteWorkItemTimeLogCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteWorkItemTimeLogCommandHandler : IRequestHandler<DeleteWorkItemTimeLogCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteWorkItemTimeLogCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteWorkItemTimeLogCommand request, CancellationToken cancellationToken)
    {
        var timeLog = await _db.WorkItemTimeLogs.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemTimeLog", request.Id);

        _db.WorkItemTimeLogs.Remove(timeLog);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
