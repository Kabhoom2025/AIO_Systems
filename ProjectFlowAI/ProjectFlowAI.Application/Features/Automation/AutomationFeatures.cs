using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Automation;

// ---------------------------------------------------------------------------
// Request shapes shared by Create/Update
// ---------------------------------------------------------------------------

public record WorkflowConditionInput(string FieldPath, WorkflowConditionOperator Operator, string Value);
public record WorkflowActionInput(WorkflowActionType ActionType, string ActionConfigJson, int Order);

// ---------------------------------------------------------------------------
// List / Get
// ---------------------------------------------------------------------------

public record ListWorkflowsQuery(Guid? ProjectId) : IRequest<IReadOnlyList<WorkflowSummaryDto>>;

public class ListWorkflowsQueryHandler : IRequestHandler<ListWorkflowsQuery, IReadOnlyList<WorkflowSummaryDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListWorkflowsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<WorkflowSummaryDto>> Handle(ListWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.WorkflowDefinitions.Include(w => w.Conditions).Include(w => w.Actions).AsQueryable();
        if (request.ProjectId.HasValue) query = query.Where(w => w.ProjectId == request.ProjectId.Value);

        var entities = await query.OrderByDescending(w => w.CreatedAt).ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<WorkflowSummaryDto>(e)).ToList();
    }
}

public record GetWorkflowQuery(Guid Id) : IRequest<WorkflowDetailDto>;

public class GetWorkflowQueryHandler : IRequestHandler<GetWorkflowQuery, WorkflowDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetWorkflowQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkflowDetailDto> Handle(GetWorkflowQuery request, CancellationToken cancellationToken)
    {
        var entity = await _db.WorkflowDefinitions.Include(w => w.Conditions).Include(w => w.Actions)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkflowDefinition", request.Id);
        return _mapper.Map<WorkflowDetailDto>(entity);
    }
}

// ---------------------------------------------------------------------------
// Create / Update / Delete
// ---------------------------------------------------------------------------

public record CreateWorkflowCommand(Guid OrganizationId, Guid? ProjectId, string Name, WorkflowTriggerType TriggerType,
    string? CronExpression, IReadOnlyList<WorkflowConditionInput> Conditions, IReadOnlyList<WorkflowActionInput> Actions,
    Guid CreatedByUserId) : IRequest<WorkflowDetailDto>;

public class CreateWorkflowCommandValidator : AbstractValidator<CreateWorkflowCommand>
{
    public CreateWorkflowCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TriggerType).IsInEnum();
        RuleFor(x => x.CronExpression).NotEmpty()
            .When(x => x.TriggerType == WorkflowTriggerType.Scheduled)
            .WithMessage("CronExpression is required when TriggerType is Scheduled.");
        RuleFor(x => x.Actions).NotEmpty().WithMessage("At least one action is required.");
        RuleForEach(x => x.Conditions).ChildRules(c =>
        {
            c.RuleFor(cc => cc.FieldPath).NotEmpty();
            c.RuleFor(cc => cc.Operator).IsInEnum();
        });
        RuleForEach(x => x.Actions).ChildRules(a =>
        {
            a.RuleFor(aa => aa.ActionType).IsInEnum();
            a.RuleFor(aa => aa.ActionConfigJson).NotEmpty();
        });
    }
}

public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateWorkflowCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkflowDetailDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);
        if (request.ProjectId.HasValue && !await _db.Projects.AnyAsync(p => p.Id == request.ProjectId.Value, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId.Value);

        var workflow = new WorkflowDefinition
        {
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            Name = request.Name,
            TriggerType = request.TriggerType,
            TriggerConfigJson = request.TriggerType == WorkflowTriggerType.Scheduled ? request.CronExpression ?? string.Empty : string.Empty,
            IsEnabled = false,
            CreatedByUserId = request.CreatedByUserId
        };
        foreach (var c in request.Conditions)
            workflow.Conditions.Add(new WorkflowCondition { FieldPath = c.FieldPath, Operator = c.Operator, Value = c.Value });
        foreach (var a in request.Actions)
            workflow.Actions.Add(new WorkflowAction { ActionType = a.ActionType, ActionConfigJson = a.ActionConfigJson, Order = a.Order });

        _db.WorkflowDefinitions.Add(workflow);
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.WorkflowDefinitions.Include(w => w.Conditions).Include(w => w.Actions)
            .FirstAsync(w => w.Id == workflow.Id, cancellationToken);
        return _mapper.Map<WorkflowDetailDto>(withNav);
    }
}

public record UpdateWorkflowCommand(Guid Id, string Name, string? CronExpression,
    IReadOnlyList<WorkflowConditionInput> Conditions, IReadOnlyList<WorkflowActionInput> Actions) : IRequest<WorkflowDetailDto>;

public class UpdateWorkflowCommandValidator : AbstractValidator<UpdateWorkflowCommand>
{
    public UpdateWorkflowCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Actions).NotEmpty();
    }
}

public class UpdateWorkflowCommandHandler : IRequestHandler<UpdateWorkflowCommand, WorkflowDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWorkflowScheduler _scheduler;

    public UpdateWorkflowCommandHandler(IProjectFlowDbContext db, IMapper mapper, IWorkflowScheduler scheduler)
    {
        _db = db; _mapper = mapper; _scheduler = scheduler;
    }

    public async Task<WorkflowDetailDto> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions.Include(w => w.Conditions).Include(w => w.Actions)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkflowDefinition", request.Id);

        workflow.Name = request.Name;
        if (workflow.TriggerType == WorkflowTriggerType.Scheduled)
            workflow.TriggerConfigJson = request.CronExpression ?? workflow.TriggerConfigJson;

        // Replace conditions/actions wholesale on update — simplest correct semantics per spec.
        foreach (var c in workflow.Conditions.ToList()) _db.WorkflowConditions.Remove(c);
        foreach (var a in workflow.Actions.ToList()) _db.WorkflowActions.Remove(a);
        workflow.Conditions.Clear();
        workflow.Actions.Clear();
        foreach (var c in request.Conditions)
            workflow.Conditions.Add(new WorkflowCondition { WorkflowDefinitionId = workflow.Id, FieldPath = c.FieldPath, Operator = c.Operator, Value = c.Value });
        foreach (var a in request.Actions)
            workflow.Actions.Add(new WorkflowAction { WorkflowDefinitionId = workflow.Id, ActionType = a.ActionType, ActionConfigJson = a.ActionConfigJson, Order = a.Order });

        await _db.SaveChangesAsync(cancellationToken);

        // Re-register the Hangfire recurring job if this workflow is an enabled Scheduled workflow
        // and its cron just changed, so the schedule takes effect immediately.
        if (workflow.IsEnabled && workflow.TriggerType == WorkflowTriggerType.Scheduled)
            _scheduler.RegisterOrUpdate(workflow.Id, workflow.TriggerConfigJson);

        var withNav = await _db.WorkflowDefinitions.Include(w => w.Conditions).Include(w => w.Actions)
            .FirstAsync(w => w.Id == workflow.Id, cancellationToken);
        return _mapper.Map<WorkflowDetailDto>(withNav);
    }
}

public record DeleteWorkflowCommand(Guid Id) : IRequest<Unit>;

public class DeleteWorkflowCommandValidator : AbstractValidator<DeleteWorkflowCommand>
{
    public DeleteWorkflowCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteWorkflowCommandHandler : IRequestHandler<DeleteWorkflowCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IWorkflowScheduler _scheduler;

    public DeleteWorkflowCommandHandler(IProjectFlowDbContext db, IWorkflowScheduler scheduler) { _db = db; _scheduler = scheduler; }

    public async Task<Unit> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkflowDefinition", request.Id);

        if (workflow.TriggerType == WorkflowTriggerType.Scheduled)
            _scheduler.Remove(workflow.Id);

        _db.WorkflowDefinitions.Remove(workflow);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Enable / Disable
// ---------------------------------------------------------------------------

public record EnableWorkflowCommand(Guid Id) : IRequest<Unit>;

public class EnableWorkflowCommandHandler : IRequestHandler<EnableWorkflowCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IWorkflowScheduler _scheduler;

    public EnableWorkflowCommandHandler(IProjectFlowDbContext db, IWorkflowScheduler scheduler) { _db = db; _scheduler = scheduler; }

    public async Task<Unit> Handle(EnableWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkflowDefinition", request.Id);

        workflow.IsEnabled = true;
        await _db.SaveChangesAsync(cancellationToken);

        if (workflow.TriggerType == WorkflowTriggerType.Scheduled)
            _scheduler.RegisterOrUpdate(workflow.Id, workflow.TriggerConfigJson);

        return Unit.Value;
    }
}

public record DisableWorkflowCommand(Guid Id) : IRequest<Unit>;

public class DisableWorkflowCommandHandler : IRequestHandler<DisableWorkflowCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IWorkflowScheduler _scheduler;

    public DisableWorkflowCommandHandler(IProjectFlowDbContext db, IWorkflowScheduler scheduler) { _db = db; _scheduler = scheduler; }

    public async Task<Unit> Handle(DisableWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkflowDefinition", request.Id);

        workflow.IsEnabled = false;
        await _db.SaveChangesAsync(cancellationToken);

        if (workflow.TriggerType == WorkflowTriggerType.Scheduled)
            _scheduler.Remove(workflow.Id);

        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Runs
// ---------------------------------------------------------------------------

public record ListWorkflowRunsQuery(Guid WorkflowId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<WorkflowRunDto>>;

public class ListWorkflowRunsQueryHandler : IRequestHandler<ListWorkflowRunsQuery, PagedResult<WorkflowRunDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListWorkflowRunsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<WorkflowRunDto>> Handle(ListWorkflowRunsQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkflowDefinitions.AnyAsync(w => w.Id == request.WorkflowId, cancellationToken))
            throw new NotFoundException("WorkflowDefinition", request.WorkflowId);

        var query = _db.WorkflowRuns.Where(r => r.WorkflowDefinitionId == request.WorkflowId).OrderByDescending(r => r.StartedAt);
        var total = await query.CountAsync(cancellationToken);
        var entities = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        var items = entities.Select(e => _mapper.Map<WorkflowRunDto>(e)).ToList();
        return new PagedResult<WorkflowRunDto>(items, total, request.Page, request.PageSize);
    }
}

// ---------------------------------------------------------------------------
// Approvals
// ---------------------------------------------------------------------------

public record ListApprovalsQuery(Guid UserId, bool AssignedToMe) : IRequest<IReadOnlyList<ApprovalSummaryDto>>;

public class ListApprovalsQueryHandler : IRequestHandler<ListApprovalsQuery, IReadOnlyList<ApprovalSummaryDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListApprovalsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<ApprovalSummaryDto>> Handle(ListApprovalsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.WorkflowApprovalRequests
            .Include(a => a.WorkflowRun).ThenInclude(r => r!.WorkflowDefinition)
            .AsQueryable();

        // AssignedToMe=true is the primary use case per spec ("for the current user's pending
        // approval requests"); AssignedToMe=false still scopes to requests the user is the
        // approver on (there is no blanket "see everyone's approvals" permission), just without
        // filtering out already-decided ones.
        query = query.Where(a => a.RequestedApproverUserId == request.UserId);
        if (request.AssignedToMe)
            query = query.Where(a => a.Status == WorkflowApprovalStatus.Pending);

        var entities = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<ApprovalSummaryDto>(e)).ToList();
    }
}

public record DecideApprovalCommand(Guid ApprovalId, bool Approve, string? Comment, Guid ActorUserId) : IRequest<Unit>;

public class DecideApprovalCommandValidator : AbstractValidator<DecideApprovalCommand>
{
    public DecideApprovalCommandValidator()
    {
        RuleFor(x => x.ApprovalId).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class DecideApprovalCommandHandler : IRequestHandler<DecideApprovalCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IWorkflowEngine _engine;

    public DecideApprovalCommandHandler(IProjectFlowDbContext db, IWorkflowEngine engine) { _db = db; _engine = engine; }

    public async Task<Unit> Handle(DecideApprovalCommand request, CancellationToken cancellationToken)
    {
        var approval = await _db.WorkflowApprovalRequests.FirstOrDefaultAsync(a => a.Id == request.ApprovalId, cancellationToken)
            ?? throw new NotFoundException("WorkflowApprovalRequest", request.ApprovalId);

        // Approval decisions are gated by "you are the RequestedApproverUserId on this specific
        // approval" — checked here in the handler, NOT by a blanket permission policy.
        if (approval.RequestedApproverUserId != request.ActorUserId)
            throw new UnauthorizedDomainException("Only the requested approver may decide this approval.");
        if (approval.Status != WorkflowApprovalStatus.Pending)
            throw new ConflictException("This approval has already been decided.");

        approval.Status = request.Approve ? WorkflowApprovalStatus.Approved : WorkflowApprovalStatus.Rejected;
        approval.DecidedAt = DateTime.UtcNow;
        approval.DecidedByUserId = request.ActorUserId;
        approval.Comment = request.Comment;
        await _db.SaveChangesAsync(cancellationToken);

        // Approved -> resumes and executes remaining actions in that WorkflowRun.
        // Rejected -> marks the run Failed and stops (handled inside ResumeRunAsync).
        await _engine.ResumeRunAsync(approval.WorkflowRunId, request.Approve, cancellationToken);
        return Unit.Value;
    }
}
