using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.TimeTracking;

// ---------------------------------------------------------------------------
// StartTimer / StopTimer / GetActiveTimer
// ---------------------------------------------------------------------------

public record StartTimerCommand(Guid WorkItemId, Guid UserId) : IRequest<ActiveTimerDto>;

public class StartTimerCommandValidator : AbstractValidator<StartTimerCommand>
{
    public StartTimerCommandValidator()
    {
        RuleFor(x => x.WorkItemId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class StartTimerCommandHandler : IRequestHandler<StartTimerCommand, ActiveTimerDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _clock;

    public StartTimerCommandHandler(IProjectFlowDbContext db, IMapper mapper, IDateTimeProvider clock)
    {
        _db = db; _mapper = mapper; _clock = clock;
    }

    public async Task<ActiveTimerDto> Handle(StartTimerCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.WorkItems.AnyAsync(w => w.Id == request.WorkItemId, cancellationToken))
            throw new NotFoundException("WorkItem", request.WorkItemId);

        if (await _db.ActiveTimers.AnyAsync(t => t.UserId == request.UserId, cancellationToken))
            throw new ConflictException("You already have a running timer. Stop it before starting a new one.");

        var timer = new ActiveTimer { UserId = request.UserId, WorkItemId = request.WorkItemId, StartedAt = _clock.UtcNow };
        _db.ActiveTimers.Add(timer);
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.ActiveTimers.Include(t => t.WorkItem).FirstAsync(t => t.Id == timer.Id, cancellationToken);
        return _mapper.Map<ActiveTimerDto>(withNav);
    }
}

public record StopTimerCommand(Guid UserId) : IRequest<WorkItemTimeLogDto>;

public class StopTimerCommandValidator : AbstractValidator<StopTimerCommand>
{
    public StopTimerCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}

/// <summary>Stops the current user's running timer (404 if none), creates a real WorkItemTimeLog via
/// LogWorkItemTimeCommand — the exact same path Phase 2's manual "log time" button uses — so the new
/// entry shows up identically in the work item's detail/activity feed, then deletes the ActiveTimer row.</summary>
public class StopTimerCommandHandler : IRequestHandler<StopTimerCommand, WorkItemTimeLogDto>
{
    private const int MinimumBillableMinutes = 1;

    private readonly IProjectFlowDbContext _db;
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _clock;

    public StopTimerCommandHandler(IProjectFlowDbContext db, IMediator mediator, IDateTimeProvider clock)
    {
        _db = db; _mediator = mediator; _clock = clock;
    }

    public async Task<WorkItemTimeLogDto> Handle(StopTimerCommand request, CancellationToken cancellationToken)
    {
        var timer = await _db.ActiveTimers.FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("ActiveTimer for user", request.UserId);

        var now = _clock.UtcNow;
        // A timer stopped within the same second it started would otherwise produce a nonsensical
        // 0-minute log entry; floor to whole minutes but never below 1 so every stopped timer leaves
        // a real, non-zero trace of the work that happened.
        var elapsedMinutes = Math.Max(MinimumBillableMinutes, (int)Math.Round((now - timer.StartedAt).TotalMinutes, MidpointRounding.AwayFromZero));

        var workItemId = timer.WorkItemId;
        var loggedDate = DateOnly.FromDateTime(timer.StartedAt);

        var timeLog = await _mediator.Send(new LogWorkItemTimeCommand(
            workItemId, request.UserId, elapsedMinutes, "Logged via timer", loggedDate), cancellationToken);

        _db.ActiveTimers.Remove(timer);
        await _db.SaveChangesAsync(cancellationToken);

        return timeLog;
    }
}

public record GetActiveTimerQuery(Guid UserId) : IRequest<ActiveTimerDto?>;

public class GetActiveTimerQueryHandler : IRequestHandler<GetActiveTimerQuery, ActiveTimerDto?>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetActiveTimerQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ActiveTimerDto?> Handle(GetActiveTimerQuery request, CancellationToken cancellationToken)
    {
        var timer = await _db.ActiveTimers.Include(t => t.WorkItem).FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);
        return timer == null ? null : _mapper.Map<ActiveTimerDto>(timer);
    }
}

// ---------------------------------------------------------------------------
// Timesheets
// ---------------------------------------------------------------------------

public record GetTimesheetQuery(Guid UserId, DateOnly From, DateOnly To) : IRequest<TimesheetDto>;

public class GetTimesheetQueryHandler : IRequestHandler<GetTimesheetQuery, TimesheetDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetTimesheetQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<TimesheetDto> Handle(GetTimesheetQuery request, CancellationToken cancellationToken)
    {
        var logs = await _db.WorkItemTimeLogs
            .Where(t => t.UserId == request.UserId && t.LoggedDate >= request.From && t.LoggedDate <= request.To)
            .Include(t => t.WorkItem).ThenInclude(w => w!.Project)
            .OrderByDescending(t => t.LoggedDate)
            .ToListAsync(cancellationToken);

        var entries = logs.Select(t => new TimesheetEntryDto(
            t.Id, t.LoggedDate, t.WorkItemId, t.WorkItem?.Title ?? string.Empty,
            t.WorkItem?.Project?.Name ?? string.Empty, t.Minutes, t.IsBillable)).ToList();

        return new TimesheetDto(entries, entries.Sum(e => e.Minutes), entries.Where(e => e.IsBillable).Sum(e => e.Minutes));
    }
}
