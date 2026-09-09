using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Sprints;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Reports;

// ---------------------------------------------------------------------------
// Shared project-health scoring — used by BOTH GetExecutiveDashboardQueryHandler's per-project
// summaries and GetProjectHealthQueryHandler, so the two endpoints can never silently diverge on
// what "Red"/"Yellow"/"Green" means. Rule: Red if overdueCount > 5 OR any WorkItem has sat in
// Blocked for more than BlockedDaysThreshold days; Yellow if overdueCount is 1-5; Green otherwise.
// ---------------------------------------------------------------------------

internal static class ProjectHealthScorer
{
    public const int BlockedDaysThreshold = 3;

    public record ScoreResult(ProjectHealthStatus Status, List<string> Reasons, int OverdueCount, int BlockedCount);

    /// <summary>project.WorkItems must already be loaded (Include) by the caller.</summary>
    public static async Task<ScoreResult> ScoreAsync(IProjectFlowDbContext db, Project project, DateTime today, CancellationToken cancellationToken)
    {
        var overdueCount = project.WorkItems.Count(w => w.Status != WorkItemStatus.Done && w.DueDate != null && w.DueDate.Value.Date < today);
        var blockedItems = project.WorkItems.Where(w => w.Status == WorkItemStatus.Blocked).ToList();
        var blockedSince = await GetBlockedSinceAsync(db, blockedItems.Select(w => w.Id).ToList(), cancellationToken);

        var reasons = new List<string>();
        if (overdueCount > 0)
        {
            var taskWord = overdueCount == 1 ? "task is" : "tasks are";
            reasons.Add($"{overdueCount} {taskWord} overdue.");
        }

        var hasLongBlocked = false;
        foreach (var item in blockedItems)
        {
            // No recorded StatusChanged->Blocked activity (e.g. seeded straight into Blocked with
            // only a "Created" row) falls back to UpdatedAt/CreatedAt as the best available
            // "blocked since" estimate, rather than silently excluding the item from scoring.
            var since = blockedSince.TryGetValue(item.Id, out var s) ? s : (item.UpdatedAt ?? item.CreatedAt);
            var days = Math.Max(0, (today - since.Date).Days);
            if (days > BlockedDaysThreshold)
            {
                hasLongBlocked = true;
                reasons.Add($"\"{item.Title}\" has been blocked for {days} day{(days == 1 ? "" : "s")}.");
            }
        }

        var status = overdueCount > 5 || hasLongBlocked
            ? ProjectHealthStatus.Red
            : overdueCount >= 1 ? ProjectHealthStatus.Yellow : ProjectHealthStatus.Green;

        return new ScoreResult(status, reasons, overdueCount, blockedItems.Count);
    }

    private static async Task<Dictionary<Guid, DateTime>> GetBlockedSinceAsync(IProjectFlowDbContext db, List<Guid> itemIds, CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0) return new Dictionary<Guid, DateTime>();

        var transitions = await db.WorkItemActivities
            .Where(a => itemIds.Contains(a.WorkItemId) && a.Action == "StatusChanged" && a.NewValue == "Blocked")
            .GroupBy(a => a.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, BlockedAt = g.Max(a => a.CreatedAt) })
            .ToListAsync(cancellationToken);

        return transitions.ToDictionary(x => x.WorkItemId, x => x.BlockedAt);
    }
}

// ---------------------------------------------------------------------------
// Executive dashboard
// ---------------------------------------------------------------------------

public record GetExecutiveDashboardQuery(Guid OrganizationId) : IRequest<ExecutiveDashboardDto>;

public class GetExecutiveDashboardQueryValidator : AbstractValidator<GetExecutiveDashboardQuery>
{
    public GetExecutiveDashboardQueryValidator() => RuleFor(x => x.OrganizationId).NotEmpty();
}

public class GetExecutiveDashboardQueryHandler : IRequestHandler<GetExecutiveDashboardQuery, ExecutiveDashboardDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetExecutiveDashboardQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<ExecutiveDashboardDto> Handle(GetExecutiveDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var projects = await _db.Projects.Where(p => p.OrganizationId == request.OrganizationId)
            .Include(p => p.WorkItems)
            .Include(p => p.Sprints)
            .ToListAsync(cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();

        var completedThisMonth = await _db.WorkItemActivities
            .Where(a => a.Action == "StatusChanged" && a.NewValue == "Done"
                && a.CreatedAt >= monthStart && a.CreatedAt < monthEnd
                && projectIds.Contains(a.WorkItem!.ProjectId))
            .Select(a => a.WorkItemId)
            .Distinct()
            .CountAsync(cancellationToken);

        var summaries = new List<ProjectHealthSummaryDto>();
        var overdueTotal = 0;
        var atRisk = 0;
        foreach (var project in projects)
        {
            var score = await ProjectHealthScorer.ScoreAsync(_db, project, today, cancellationToken);
            overdueTotal += score.OverdueCount;
            if (score.Status != ProjectHealthStatus.Green) atRisk++;

            var totalItems = project.WorkItems.Count;
            var doneItems = project.WorkItems.Count(w => w.Status == WorkItemStatus.Done);
            var progressPercent = totalItems == 0 ? 0.0 : (double)doneItems / totalItems * 100.0;
            var activeSprint = project.Sprints.FirstOrDefault(s => s.Status == SprintStatus.Active);

            summaries.Add(new ProjectHealthSummaryDto(project.Id, project.Name, score.Status, progressPercent, score.OverdueCount, activeSprint?.Name));
        }

        var totalWorkItems = projects.Sum(p => p.WorkItems.Count);

        return new ExecutiveDashboardDto(projects.Count, projects.Count(p => p.Status == ProjectStatus.Active),
            totalWorkItems, completedThisMonth, overdueTotal, atRisk, summaries);
    }
}

// ---------------------------------------------------------------------------
// Sprint dashboard (burndown, blocked items, open retro action items)
// ---------------------------------------------------------------------------

public record GetSprintDashboardQuery(Guid SprintId) : IRequest<SprintDashboardDto>;

public class GetSprintDashboardQueryValidator : AbstractValidator<GetSprintDashboardQuery>
{
    public GetSprintDashboardQueryValidator() => RuleFor(x => x.SprintId).NotEmpty();
}

public class GetSprintDashboardQueryHandler : IRequestHandler<GetSprintDashboardQuery, SprintDashboardDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetSprintDashboardQueryHandler(IProjectFlowDbContext db, IMapper mapper, IMediator mediator)
    {
        _db = db; _mapper = mapper; _mediator = mediator;
    }

    public async Task<SprintDashboardDto> Handle(GetSprintDashboardQuery request, CancellationToken cancellationToken)
    {
        var sprint = await _db.Sprints.Include(s => s.WorkItems).FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken)
            ?? throw new NotFoundException("Sprint", request.SprintId);

        var sprintDto = _mapper.Map<SprintDto>(sprint);

        // Reuses Phase 3's own burndown handler internally rather than reimplementing the
        // day-by-day remaining-points calculation here.
        var burndown = await _mediator.Send(new GetSprintBurndownQuery(request.SprintId), cancellationToken);

        var blockedEntities = await WorkItemIncludes.WithListNavigations(
                _db.WorkItems.Where(w => w.SprintId == request.SprintId && w.Status == WorkItemStatus.Blocked))
            .ToListAsync(cancellationToken);
        var blockedItems = blockedEntities.Select(e => _mapper.Map<WorkItemDto>(e)).ToList();

        var openRetroActionItemCount = await _db.RetrospectiveNotes
            .CountAsync(n => n.SprintId == request.SprintId && n.Category == RetrospectiveCategory.ActionItem, cancellationToken);

        return new SprintDashboardDto(sprintDto, burndown, blockedItems, openRetroActionItemCount);
    }
}

// ---------------------------------------------------------------------------
// Cycle time / lead time
// ---------------------------------------------------------------------------

public record GetCycleTimeReportQuery(Guid ProjectId, DateTime From, DateTime To) : IRequest<CycleTimeReportDto>;

public class GetCycleTimeReportQueryValidator : AbstractValidator<GetCycleTimeReportQuery>
{
    public GetCycleTimeReportQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
    }
}

/// <summary>
/// Judgment calls (documented per the spec's request):
///  - leadTimeHours = hours from WorkItem.CreatedAt to the (most recent) StatusChanged activity
///    row whose NewValue == "Done".
///  - "Work started" (for cycleTimeHours) = the FIRST StatusChanged activity row whose NewValue is
///    anything other than Backlog/ToDo/Done — not strictly "InProgress", since an item can jump
///    straight from ToDo to CodeReview/Testing/Blocked without ever passing through InProgress, and
///    that is still "work started" in spirit. Done itself is excluded too, or an item that jumps
///    straight Backlog -> Done would misread its own finish transition as its start (0-hour cycle
///    time instead of "no cycle time at all"). cycleTimeHours = hours from that first qualifying
///    transition to the Done transition.
///  - An item with NO recorded qualifying transition (e.g. Backlog -> Done directly) has no
///    well-defined "work started" instant, so it is EXCLUDED from the cycle-time average (its
///    CycleTimeHours is null) but still INCLUDED in the lead-time average.
/// </summary>
public class GetCycleTimeReportQueryHandler : IRequestHandler<GetCycleTimeReportQuery, CycleTimeReportDto>
{
    // Excludes Done as well as Backlog/ToDo: a StatusChanged row's NewValue == "Done" is the FINISH
    // transition, not a "work started" candidate — without excluding it here, an item that jumped
    // straight Backlog/ToDo -> Done would have its own Done transition misread as its "start",
    // silently giving it a bogus 0-hour cycle time instead of being correctly excluded (caught by
    // CycleTimeReportHandlerTests before this shipped).
    private static readonly HashSet<string> NonStartStatuses = new()
        { nameof(WorkItemStatus.Backlog), nameof(WorkItemStatus.ToDo), nameof(WorkItemStatus.Done) };

    private readonly IProjectFlowDbContext _db;

    public GetCycleTimeReportQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<CycleTimeReportDto> Handle(GetCycleTimeReportQuery request, CancellationToken cancellationToken)
    {
        var doneTransitions = await _db.WorkItemActivities
            .Where(a => a.Action == "StatusChanged" && a.NewValue == "Done"
                && a.WorkItem!.ProjectId == request.ProjectId
                && a.CreatedAt >= request.From && a.CreatedAt <= request.To)
            .GroupBy(a => a.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, DoneAt = g.Max(a => a.CreatedAt) })
            .ToListAsync(cancellationToken);

        if (doneTransitions.Count == 0)
            return new CycleTimeReportDto(null, null, new List<CycleTimeWorkItemDto>());

        var itemIds = doneTransitions.Select(d => d.WorkItemId).ToList();
        var items = await _db.WorkItems.Where(w => itemIds.Contains(w.Id))
            .Select(w => new { w.Id, w.Title, w.CreatedAt }).ToListAsync(cancellationToken);
        var itemById = items.ToDictionary(i => i.Id);

        var workStartedTransitions = await _db.WorkItemActivities
            .Where(a => itemIds.Contains(a.WorkItemId) && a.Action == "StatusChanged"
                && a.NewValue != null && !NonStartStatuses.Contains(a.NewValue))
            .GroupBy(a => a.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, StartedAt = g.Min(a => a.CreatedAt) })
            .ToListAsync(cancellationToken);
        var startedByItem = workStartedTransitions.ToDictionary(x => x.WorkItemId, x => x.StartedAt);

        var results = new List<CycleTimeWorkItemDto>();
        foreach (var d in doneTransitions)
        {
            if (!itemById.TryGetValue(d.WorkItemId, out var item)) continue;

            var leadTimeHours = (d.DoneAt - item.CreatedAt).TotalHours;
            double? cycleTimeHours = startedByItem.TryGetValue(d.WorkItemId, out var startedAt)
                ? (d.DoneAt - startedAt).TotalHours
                : null;

            results.Add(new CycleTimeWorkItemDto(d.WorkItemId, item.Title, leadTimeHours, cycleTimeHours));
        }

        // Null (not 0) when there's no data — a reported "0 hours" reads as "completed instantly",
        // which is a materially different (and wrong) claim from "no eligible items in this range".
        double? averageLeadTimeHours = results.Count > 0 ? results.Average(r => r.LeadTimeHours) : null;
        var withCycleTime = results.Where(r => r.CycleTimeHours.HasValue).ToList();
        double? averageCycleTimeHours = withCycleTime.Count > 0 ? withCycleTime.Average(r => r.CycleTimeHours!.Value) : null;

        return new CycleTimeReportDto(averageLeadTimeHours, averageCycleTimeHours, results);
    }
}

// ---------------------------------------------------------------------------
// Single-project health (shares ProjectHealthScorer with the executive dashboard)
// ---------------------------------------------------------------------------

public record GetProjectHealthQuery(Guid ProjectId) : IRequest<ProjectHealthDto>;

public class GetProjectHealthQueryValidator : AbstractValidator<GetProjectHealthQuery>
{
    public GetProjectHealthQueryValidator() => RuleFor(x => x.ProjectId).NotEmpty();
}

public class GetProjectHealthQueryHandler : IRequestHandler<GetProjectHealthQuery, ProjectHealthDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetProjectHealthQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<ProjectHealthDto> Handle(GetProjectHealthQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.Include(p => p.WorkItems).Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var today = DateTime.UtcNow.Date;
        var score = await ProjectHealthScorer.ScoreAsync(_db, project, today, cancellationToken);

        var activeSprint = project.Sprints.FirstOrDefault(s => s.Status == SprintStatus.Active);
        double? sprintProgressPercent = null;
        if (activeSprint != null)
        {
            var sprintItems = project.WorkItems.Where(w => w.SprintId == activeSprint.Id).ToList();
            var totalPoints = sprintItems.Sum(w => w.StoryPoints ?? 0);
            var completedPoints = sprintItems.Where(w => w.Status == WorkItemStatus.Done).Sum(w => w.StoryPoints ?? 0);
            sprintProgressPercent = totalPoints == 0 ? 0.0 : (double)completedPoints / totalPoints * 100.0;
        }

        return new ProjectHealthDto(score.Status, score.Reasons, score.OverdueCount, score.BlockedCount, sprintProgressPercent);
    }
}

// ---------------------------------------------------------------------------
// Productivity (throughput by week/month bucket)
// ---------------------------------------------------------------------------

public record GetProductivityReportQuery(Guid ProjectId, DateTime From, DateTime To, ProductivityBucket Bucket = ProductivityBucket.Week)
    : IRequest<ProductivityReportDto>;

public class GetProductivityReportQueryValidator : AbstractValidator<GetProductivityReportQuery>
{
    public GetProductivityReportQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
    }
}

public class GetProductivityReportQueryHandler : IRequestHandler<GetProductivityReportQuery, ProductivityReportDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetProductivityReportQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<ProductivityReportDto> Handle(GetProductivityReportQuery request, CancellationToken cancellationToken)
    {
        var doneTransitions = await _db.WorkItemActivities
            .Where(a => a.Action == "StatusChanged" && a.NewValue == "Done"
                && a.WorkItem!.ProjectId == request.ProjectId
                && a.CreatedAt >= request.From && a.CreatedAt <= request.To)
            .GroupBy(a => a.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, DoneAt = g.Max(a => a.CreatedAt) })
            .ToListAsync(cancellationToken);

        var itemIds = doneTransitions.Select(d => d.WorkItemId).ToList();
        var points = itemIds.Count == 0
            ? new List<KeyValuePair<Guid, int>>()
            : (await _db.WorkItems.Where(w => itemIds.Contains(w.Id)).Select(w => new { w.Id, w.StoryPoints }).ToListAsync(cancellationToken))
                .Select(p => new KeyValuePair<Guid, int>(p.Id, p.StoryPoints ?? 0)).ToList();
        var pointsById = points.ToDictionary(p => p.Key, p => p.Value);

        var buckets = new Dictionary<DateOnly, (int Count, int Points)>();
        foreach (var d in doneTransitions)
        {
            var periodStart = request.Bucket == ProductivityBucket.Month
                ? new DateOnly(d.DoneAt.Year, d.DoneAt.Month, 1)
                : StartOfWeek(DateOnly.FromDateTime(d.DoneAt));

            var current = buckets.TryGetValue(periodStart, out var existing) ? existing : (Count: 0, Points: 0);
            buckets[periodStart] = (current.Count + 1, current.Points + pointsById.GetValueOrDefault(d.WorkItemId));
        }

        var result = buckets.OrderBy(kv => kv.Key)
            .Select(kv => new ProductivityBucketDto(kv.Key, kv.Value.Count, kv.Value.Points))
            .ToList();

        return new ProductivityReportDto(result);
    }

    /// <summary>Monday-based ISO week start.</summary>
    private static DateOnly StartOfWeek(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}

// ---------------------------------------------------------------------------
// Resource utilization
// ---------------------------------------------------------------------------

public record GetResourceUtilizationReportQuery(Guid OrganizationId, Guid? ProjectId, DateTime From, DateTime To)
    : IRequest<ResourceUtilizationReportDto>;

public class GetResourceUtilizationReportQueryValidator : AbstractValidator<GetResourceUtilizationReportQuery>
{
    public GetResourceUtilizationReportQueryValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
    }
}

public class GetResourceUtilizationReportQueryHandler : IRequestHandler<GetResourceUtilizationReportQuery, ResourceUtilizationReportDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetResourceUtilizationReportQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<ResourceUtilizationReportDto> Handle(GetResourceUtilizationReportQuery request, CancellationToken cancellationToken)
    {
        var projectsQuery = _db.Projects.Where(p => p.OrganizationId == request.OrganizationId);
        if (request.ProjectId.HasValue) projectsQuery = projectsQuery.Where(p => p.Id == request.ProjectId.Value);
        var projectIds = await projectsQuery.Select(p => p.Id).ToListAsync(cancellationToken);

        var assignedItems = await _db.WorkItems
            .Where(w => projectIds.Contains(w.ProjectId) && w.AssigneeUserId != null
                && w.DueDate != null && w.DueDate >= request.From && w.DueDate <= request.To)
            .Select(w => new { w.Id, AssigneeUserId = w.AssigneeUserId!.Value, w.EstimatedHours })
            .ToListAsync(cancellationToken);

        var fromDate = DateOnly.FromDateTime(request.From);
        var toDate = DateOnly.FromDateTime(request.To);
        var timeLogs = await _db.WorkItemTimeLogs
            .Where(t => projectIds.Contains(t.WorkItem!.ProjectId) && t.LoggedDate >= fromDate && t.LoggedDate <= toDate)
            .Select(t => new { t.UserId, t.Minutes })
            .ToListAsync(cancellationToken);

        var userIds = assignedItems.Select(a => a.AssigneeUserId).Union(timeLogs.Select(t => t.UserId)).Distinct().ToList();
        if (userIds.Count == 0) return new ResourceUtilizationReportDto(new List<ResourceUtilizationRowDto>());

        var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, cancellationToken);

        var rows = userIds.Select(userId =>
        {
            var estimatedHours = assignedItems.Where(a => a.AssigneeUserId == userId).Sum(a => a.EstimatedHours ?? 0m);
            var loggedHours = timeLogs.Where(t => t.UserId == userId).Sum(t => t.Minutes) / 60m;
            var taskCount = assignedItems.Count(a => a.AssigneeUserId == userId);
            // Never divide by zero: no estimated hours means utilization is undefined, not 0 or infinite.
            double? utilizationPercent = estimatedHours == 0m ? null : (double)(loggedHours / estimatedHours * 100m);
            var user = users.GetValueOrDefault(userId);
            var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
            return new ResourceUtilizationRowDto(userId, userName, estimatedHours, loggedHours, taskCount, utilizationPercent);
        }).OrderBy(r => r.UserName).ToList();

        return new ResourceUtilizationReportDto(rows);
    }
}

// ---------------------------------------------------------------------------
// Cost analysis
// ---------------------------------------------------------------------------

public record GetCostAnalysisReportQuery(Guid ProjectId, DateTime From, DateTime To) : IRequest<CostAnalysisReportDto>;

public class GetCostAnalysisReportQueryValidator : AbstractValidator<GetCostAnalysisReportQuery>
{
    public GetCostAnalysisReportQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
    }
}

public class GetCostAnalysisReportQueryHandler : IRequestHandler<GetCostAnalysisReportQuery, CostAnalysisReportDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetCostAnalysisReportQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<CostAnalysisReportDto> Handle(GetCostAnalysisReportQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var fromDate = DateOnly.FromDateTime(request.From);
        var toDate = DateOnly.FromDateTime(request.To);
        var logs = await _db.WorkItemTimeLogs
            .Where(t => t.WorkItem!.ProjectId == request.ProjectId && t.LoggedDate >= fromDate && t.LoggedDate <= toDate)
            .Select(t => new { t.UserId, t.Minutes, t.IsBillable })
            .ToListAsync(cancellationToken);

        var totalBillableHours = logs.Where(l => l.IsBillable).Sum(l => l.Minutes) / 60m;
        var totalNonBillableHours = logs.Where(l => !l.IsBillable).Sum(l => l.Minutes) / 60m;
        // DefaultHourlyRate is optional (Project.DefaultHourlyRate is nullable) — never fabricate a
        // rate when it isn't set, just report 0 cost.
        var rate = project.DefaultHourlyRate ?? 0m;
        var totalCost = totalBillableHours * rate;

        var userIds = logs.Select(l => l.UserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<Guid, User>()
            : await _db.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, cancellationToken);

        var byUser = userIds.Select(userId =>
        {
            var billableHours = logs.Where(l => l.UserId == userId && l.IsBillable).Sum(l => l.Minutes) / 60m;
            var cost = billableHours * rate;
            var user = users.GetValueOrDefault(userId);
            var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
            return new CostAnalysisUserRowDto(userId, userName, billableHours, cost);
        }).OrderByDescending(r => r.Cost).ToList();

        return new CostAnalysisReportDto(totalBillableHours, totalNonBillableHours, totalCost,
            project.DefaultHourlyRate.HasValue, byUser);
    }
}
