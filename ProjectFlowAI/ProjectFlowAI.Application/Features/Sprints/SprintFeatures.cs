using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Automation;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Sprints;

// ---------------------------------------------------------------------------
// CreateSprint / UpdateSprint / DeleteSprint
// ---------------------------------------------------------------------------

public record CreateSprintCommand(Guid ProjectId, string Name, string? Goal, DateTime StartDate, DateTime EndDate) : IRequest<SprintDto>;

public class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
{
    public CreateSprintCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.");
    }
}

public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, SprintDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateSprintCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<SprintDto> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var sprint = new Sprint
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            Goal = request.Goal,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = SprintStatus.Planned
        };
        _db.Sprints.Add(sprint);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<SprintDto>(sprint);
    }
}

public record UpdateSprintCommand(Guid Id, string Name, string? Goal, DateTime StartDate, DateTime EndDate) : IRequest<SprintDto>;

public class UpdateSprintCommandValidator : AbstractValidator<UpdateSprintCommand>
{
    public UpdateSprintCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.");
    }
}

public class UpdateSprintCommandHandler : IRequestHandler<UpdateSprintCommand, SprintDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateSprintCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<SprintDto> Handle(UpdateSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.Include(s => s.WorkItems).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.Id);

        sprint.Name = request.Name;
        sprint.Goal = request.Goal;
        sprint.StartDate = request.StartDate;
        sprint.EndDate = request.EndDate;
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<SprintDto>(sprint);
    }
}

public record DeleteSprintCommand(Guid Id) : IRequest<Unit>;

public class DeleteSprintCommandValidator : AbstractValidator<DeleteSprintCommand>
{
    public DeleteSprintCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteSprintCommandHandler : IRequestHandler<DeleteSprintCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteSprintCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.Id);

        // Work items assigned to a deleted sprint fall back to the backlog rather than being deleted.
        var assigned = await _db.WorkItems.Where(w => w.SprintId == sprint.Id).ToListAsync(cancellationToken);
        foreach (var item in assigned) item.SprintId = null;

        _db.Sprints.Remove(sprint);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// StartSprint / CompleteSprint
// ---------------------------------------------------------------------------

public record StartSprintCommand(Guid Id) : IRequest<SprintDto>;

public class StartSprintCommandValidator : AbstractValidator<StartSprintCommand>
{
    public StartSprintCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class StartSprintCommandHandler : IRequestHandler<StartSprintCommand, SprintDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationDispatcher _notifications;
    private readonly IWorkflowEngine _workflowEngine;

    public StartSprintCommandHandler(IProjectFlowDbContext db, IMapper mapper, INotificationDispatcher notifications, IWorkflowEngine workflowEngine)
    {
        _db = db; _mapper = mapper; _notifications = notifications; _workflowEngine = workflowEngine;
    }

    public async Task<SprintDto> Handle(StartSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.Include(s => s.WorkItems).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.Id);

        var alreadyActive = await _db.Sprints.AnyAsync(
            s => s.ProjectId == sprint.ProjectId && s.Status == SprintStatus.Active && s.Id != sprint.Id, cancellationToken);
        if (alreadyActive)
            throw new ConflictException("This project already has another Active sprint. Complete it before starting a new one.");

        sprint.Status = SprintStatus.Active;
        await _db.SaveChangesAsync(cancellationToken);

        var memberUserIds = await _db.ProjectMembers.Where(m => m.ProjectId == sprint.ProjectId).Select(m => m.UserId).ToListAsync(cancellationToken);
        foreach (var userId in memberUserIds)
            await _notifications.DispatchAsync(userId, NotificationType.SprintStarted,
                "Sprint started", $"Sprint \"{sprint.Name}\" has started.",
                $"/projects/{sprint.ProjectId}/sprints/{sprint.Id}", cancellationToken);

        // SprintStarted has no single triggering WorkItem, so conditions (which read fields off a
        // WorkItem) are skipped by the engine — only workflows with zero conditions fire on this trigger.
        await _workflowEngine.EvaluateTriggerAsync(WorkflowTriggerType.SprintStarted, sprint.ProjectId, null, cancellationToken);

        return _mapper.Map<SprintDto>(sprint);
    }
}

public record CompleteSprintCommand(Guid Id) : IRequest<SprintDto>;

public class CompleteSprintCommandValidator : AbstractValidator<CompleteSprintCommand>
{
    public CompleteSprintCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class CompleteSprintCommandHandler : IRequestHandler<CompleteSprintCommand, SprintDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CompleteSprintCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<SprintDto> Handle(CompleteSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.Include(s => s.WorkItems).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.Id);

        sprint.Status = SprintStatus.Completed;

        // Anything not Done returns to the backlog — the sprint's completed WorkItemCount/Points
        // snapshot (for velocity reporting) is still derivable later since Done items keep SprintId.
        foreach (var item in sprint.WorkItems.Where(w => w.Status != WorkItemStatus.Done))
            item.SprintId = null;

        await _db.SaveChangesAsync(cancellationToken);

        var reloaded = await _db.Sprints.Include(s => s.WorkItems).FirstAsync(s => s.Id == sprint.Id, cancellationToken);
        return _mapper.Map<SprintDto>(reloaded);
    }
}

// ---------------------------------------------------------------------------
// Retrospective
// ---------------------------------------------------------------------------

public record AddRetrospectiveNoteCommand(Guid SprintId, RetrospectiveCategory Category, string Text, Guid CreatedByUserId) : IRequest<RetrospectiveNoteDto>;

public class AddRetrospectiveNoteCommandValidator : AbstractValidator<AddRetrospectiveNoteCommand>
{
    public AddRetrospectiveNoteCommandValidator()
    {
        RuleFor(x => x.SprintId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
    }
}

public class AddRetrospectiveNoteCommandHandler : IRequestHandler<AddRetrospectiveNoteCommand, RetrospectiveNoteDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddRetrospectiveNoteCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<RetrospectiveNoteDto> Handle(AddRetrospectiveNoteCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Sprints.AnyAsync(s => s.Id == request.SprintId, cancellationToken))
            throw new NotFoundException("Sprint", request.SprintId);

        var note = new RetrospectiveNote
        {
            SprintId = request.SprintId,
            Category = request.Category,
            Text = request.Text,
            CreatedByUserId = request.CreatedByUserId
        };
        _db.RetrospectiveNotes.Add(note);
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.RetrospectiveNotes.Include(n => n.CreatedByUser).FirstAsync(n => n.Id == note.Id, cancellationToken);
        return _mapper.Map<RetrospectiveNoteDto>(withNav);
    }
}

public record DeleteRetrospectiveNoteCommand(Guid Id) : IRequest<Unit>;

public class DeleteRetrospectiveNoteCommandValidator : AbstractValidator<DeleteRetrospectiveNoteCommand>
{
    public DeleteRetrospectiveNoteCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteRetrospectiveNoteCommandHandler : IRequestHandler<DeleteRetrospectiveNoteCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteRetrospectiveNoteCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteRetrospectiveNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _db.RetrospectiveNotes.FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("RetrospectiveNote", request.Id);

        _db.RetrospectiveNotes.Remove(note);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Queries: List / Board / Velocity / Burndown / Burnup / Retrospective
// ---------------------------------------------------------------------------

public record ListSprintsQuery(Guid? ProjectId, SprintStatus? Status, int Page = 1, int PageSize = 20,
    string? SortBy = null, string? SortDir = "asc") : IRequest<PagedResult<SprintDto>>;

public class ListSprintsQueryHandler : IRequestHandler<ListSprintsQuery, PagedResult<SprintDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListSprintsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<SprintDto>> Handle(ListSprintsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Sprints.AsQueryable();
        if (request.ProjectId.HasValue) query = query.Where(s => s.ProjectId == request.ProjectId.Value);
        if (request.Status.HasValue) query = query.Where(s => s.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Sprint.StartDate));

        var entities = await sorted.Include(s => s.WorkItems)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(e => _mapper.Map<SprintDto>(e)).ToList();
        return new PagedResult<SprintDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetSprintBoardQuery(Guid SprintId) : IRequest<SprintBoardDto>;

public class GetSprintBoardQueryHandler : IRequestHandler<GetSprintBoardQuery, SprintBoardDto>
{
    private static readonly WorkItemStatus[] AllStatuses =
    {
        WorkItemStatus.Backlog, WorkItemStatus.ToDo, WorkItemStatus.InProgress,
        WorkItemStatus.CodeReview, WorkItemStatus.Testing, WorkItemStatus.Blocked, WorkItemStatus.Done
    };

    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetSprintBoardQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<SprintBoardDto> Handle(GetSprintBoardQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Sprints.AnyAsync(s => s.Id == request.SprintId, cancellationToken))
            throw new NotFoundException("Sprint", request.SprintId);

        var entities = await WorkItemIncludes.WithListNavigations(_db.WorkItems.Where(w => w.SprintId == request.SprintId))
            .OrderBy(w => w.Position).ToListAsync(cancellationToken);

        var dtosByStatus = entities.Select(e => _mapper.Map<DTOs.WorkItemDto>(e)).GroupBy(d => d.Status)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<DTOs.WorkItemDto>)g.OrderBy(d => d.Position).ToList());

        var columns = AllStatuses
            .Select(status => new KanbanColumnDto(status, dtosByStatus.TryGetValue(status, out var items) ? items : new List<DTOs.WorkItemDto>()))
            .ToList();

        return new SprintBoardDto(request.SprintId, columns);
    }
}

public record GetProjectVelocityQuery(Guid ProjectId) : IRequest<VelocityDto>;

public class GetProjectVelocityQueryHandler : IRequestHandler<GetProjectVelocityQuery, VelocityDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetProjectVelocityQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<VelocityDto> Handle(GetProjectVelocityQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var sprints = await _db.Sprints
            .Where(s => s.ProjectId == request.ProjectId && s.Status == SprintStatus.Completed)
            .Include(s => s.WorkItems)
            .OrderByDescending(s => s.EndDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        // "Committed" = every item that was ever in the sprint at completion time (we only track
        // current WorkItem.SprintId, so committed == whatever remains linked at completion, which
        // for a Completed sprint is exactly its Done items since CompleteSprint returns the rest to
        // backlog) — committed and completed therefore coincide here; this is a deliberate,
        // documented simplification since we don't persist a separate "sprint scope at commit time" snapshot.
        var result = sprints.Select(s =>
        {
            var completed = s.WorkItems.Where(w => w.Status == WorkItemStatus.Done).Sum(w => w.StoryPoints ?? 0);
            var committed = s.WorkItems.Sum(w => w.StoryPoints ?? 0);
            return new VelocitySprintDto(s.Id, s.Name, committed, completed);
        }).ToList();

        return new VelocityDto(result);
    }
}

public record GetSprintBurndownQuery(Guid SprintId) : IRequest<BurndownDto>;

public class GetSprintBurndownQueryHandler : IRequestHandler<GetSprintBurndownQuery, BurndownDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetSprintBurndownQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<BurndownDto> Handle(GetSprintBurndownQuery request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.Include(s => s.WorkItems).FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.SprintId);

        var items = sprint.WorkItems.ToList();
        var totalPoints = items.Sum(w => w.StoryPoints ?? 0);

        // Real "Done" transition timestamps per WorkItem, reconstructed from WorkItemActivity's
        // StatusChanged/NewValue == "Done" rows — the most recent such transition per item, since a
        // ticket can bounce out of Done and back in. If no such activity exists (e.g. seed data with
        // no activity trail) the item is treated as "still not done" for burndown purposes rather
        // than guessing a fake completion date.
        var itemIds = items.Select(w => w.Id).ToList();
        var doneTransitions = await _db.WorkItemActivities
            .Where(a => itemIds.Contains(a.WorkItemId) && a.Action == "StatusChanged" && a.NewValue == "Done")
            .GroupBy(a => a.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, DoneAt = g.Max(a => a.CreatedAt) })
            .ToListAsync(cancellationToken);
        var doneAtByItem = doneTransitions.ToDictionary(x => x.WorkItemId, x => x.DoneAt);

        var startDate = DateOnly.FromDateTime(sprint.StartDate);
        var endDate = DateOnly.FromDateTime(sprint.EndDate);
        var totalDays = Math.Max(1, endDate.DayNumber - startDate.DayNumber);

        var days = new List<BurndownPointDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var endOfDayUtc = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            var remaining = items.Sum(w =>
            {
                var points = w.StoryPoints ?? 0;
                var isDoneByThisDay = doneAtByItem.TryGetValue(w.Id, out var doneAt) && doneAt <= endOfDayUtc;
                return isDoneByThisDay ? 0 : points;
            });
            var elapsedDays = date.DayNumber - startDate.DayNumber;
            var ideal = totalPoints * (1.0 - (double)elapsedDays / totalDays);
            days.Add(new BurndownPointDto(date, remaining, Math.Max(0, ideal)));
        }

        return new BurndownDto(days);
    }
}

public record GetSprintBurnupQuery(Guid SprintId) : IRequest<BurnupDto>;

public class GetSprintBurnupQueryHandler : IRequestHandler<GetSprintBurnupQuery, BurnupDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetSprintBurnupQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<BurnupDto> Handle(GetSprintBurnupQuery request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.Include(s => s.WorkItems).FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.SprintId);

        var items = sprint.WorkItems.ToList();
        // Simplification (judgment call, documented in the report): totalScopePoints is a flat
        // "current total assigned to the sprint" for every day, rather than reconstructing
        // scope-added-mid-sprint from an activity trail of SprintId changes (which isn't logged
        // today) — completedPoints below IS real, activity-log-derived data.
        var totalScopePoints = items.Sum(w => w.StoryPoints ?? 0);

        var itemIds = items.Select(w => w.Id).ToList();
        var doneTransitions = await _db.WorkItemActivities
            .Where(a => itemIds.Contains(a.WorkItemId) && a.Action == "StatusChanged" && a.NewValue == "Done")
            .GroupBy(a => a.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, DoneAt = g.Max(a => a.CreatedAt) })
            .ToListAsync(cancellationToken);
        var doneAtByItem = doneTransitions.ToDictionary(x => x.WorkItemId, x => x.DoneAt);

        var startDate = DateOnly.FromDateTime(sprint.StartDate);
        var endDate = DateOnly.FromDateTime(sprint.EndDate);

        var days = new List<BurnupPointDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var endOfDayUtc = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            var completed = items.Sum(w =>
            {
                var points = w.StoryPoints ?? 0;
                var isDoneByThisDay = doneAtByItem.TryGetValue(w.Id, out var doneAt) && doneAt <= endOfDayUtc;
                return isDoneByThisDay ? points : 0;
            });
            days.Add(new BurnupPointDto(date, completed, totalScopePoints));
        }

        return new BurnupDto(days);
    }
}

public record GetSprintRetrospectiveQuery(Guid SprintId) : IRequest<RetrospectiveDto>;

public class GetSprintRetrospectiveQueryHandler : IRequestHandler<GetSprintRetrospectiveQuery, RetrospectiveDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetSprintRetrospectiveQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<RetrospectiveDto> Handle(GetSprintRetrospectiveQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Sprints.AnyAsync(s => s.Id == request.SprintId, cancellationToken))
            throw new NotFoundException("Sprint", request.SprintId);

        var notes = await _db.RetrospectiveNotes.Include(n => n.CreatedByUser)
            .Where(n => n.SprintId == request.SprintId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return new RetrospectiveDto(notes.Select(n => _mapper.Map<RetrospectiveNoteDto>(n)).ToList());
    }
}
