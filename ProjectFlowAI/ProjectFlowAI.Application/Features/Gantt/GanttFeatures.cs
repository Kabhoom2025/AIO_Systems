using System.Text;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Gantt;

/// <summary>Shared "effective start/end/progress" rules so the live Gantt chart, baseline
/// snapshots, and CSV export never disagree about what a WorkItem's schedule actually is.</summary>
internal static class GanttCalculator
{
    public static DateTime EffectiveStart(WorkItem w) => (w.StartDate ?? w.CreatedAt).Date;

    public static DateTime EffectiveEnd(WorkItem w) => w.DueDate?.Date ?? EffectiveStart(w).AddDays(1);

    public static double Progress(WorkItem w)
    {
        if (w.Status == WorkItemStatus.Done) return 1.0;
        if (w.Status == WorkItemStatus.Backlog) return 0.0;
        if (w.ChecklistItems.Count > 0)
            return (double)w.ChecklistItems.Count(c => c.IsDone) / w.ChecklistItems.Count;
        return 0.5;
    }
}

public record GetGanttChartQuery(Guid ProjectId) : IRequest<GanttChartDto>;

public class GetGanttChartQueryHandler : IRequestHandler<GetGanttChartQuery, GanttChartDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetGanttChartQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<GanttChartDto> Handle(GetGanttChartQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var workItems = await _db.WorkItems.Where(w => w.ProjectId == request.ProjectId)
            .Include(w => w.ChecklistItems)
            .Include(w => w.Dependencies)
            .ToListAsync(cancellationToken);

        var items = workItems.Select(w => new GanttItemDto(
            w.Id, w.Title, GanttCalculator.EffectiveStart(w), GanttCalculator.EffectiveEnd(w),
            GanttCalculator.Progress(w), w.Status, w.Priority, w.ParentWorkItemId,
            w.Dependencies.Select(d => new GanttDependencyDto(d.DependsOnWorkItemId, d.DependencyType)).ToList()
        )).ToList();

        var milestones = await _db.Milestones.Where(m => m.ProjectId == request.ProjectId)
            .Select(m => new GanttMilestoneDto(m.Id, m.Name, m.DueDate))
            .ToListAsync(cancellationToken);

        return new GanttChartDto(items, milestones);
    }
}

public record GetCriticalPathQuery(Guid ProjectId) : IRequest<CriticalPathDto>;

/// <summary>Longest path (by summed duration) through the project's "Blocks"-type WorkItemDependency
/// graph, via topological sort + longest-path DP. WorkItemDependency.WorkItemId "depends on"
/// DependsOnWorkItemId, i.e. the edge runs DependsOnWorkItemId -> WorkItemId in schedule order.</summary>
public class GetCriticalPathQueryHandler : IRequestHandler<GetCriticalPathQuery, CriticalPathDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetCriticalPathQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<CriticalPathDto> Handle(GetCriticalPathQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var workItems = await _db.WorkItems.Where(w => w.ProjectId == request.ProjectId).ToListAsync(cancellationToken);
        if (workItems.Count == 0) return new CriticalPathDto(Array.Empty<Guid>());

        var itemIds = workItems.Select(w => w.Id).ToHashSet();
        var edges = await _db.WorkItemDependencies
            .Where(d => d.DependencyType == WorkItemDependencyType.Blocks && itemIds.Contains(d.WorkItemId) && itemIds.Contains(d.DependsOnWorkItemId))
            .ToListAsync(cancellationToken);

        if (edges.Count == 0) return new CriticalPathDto(Array.Empty<Guid>());

        var durationByItem = workItems.ToDictionary(w => w.Id, w => (GanttCalculator.EffectiveEnd(w) - GanttCalculator.EffectiveStart(w)).TotalDays);
        // predecessors[X] = list of items that X depends on (must finish before X starts)
        var predecessors = itemIds.ToDictionary(id => id, _ => new List<Guid>());
        var successors = itemIds.ToDictionary(id => id, _ => new List<Guid>());
        foreach (var e in edges)
        {
            predecessors[e.WorkItemId].Add(e.DependsOnWorkItemId);
            successors[e.DependsOnWorkItemId].Add(e.WorkItemId);
        }

        // Kahn's algorithm topological sort
        var inDegree = itemIds.ToDictionary(id => id, id => predecessors[id].Count);
        var queue = new Queue<Guid>(itemIds.Where(id => inDegree[id] == 0));
        var topoOrder = new List<Guid>();
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            topoOrder.Add(node);
            foreach (var succ in successors[node])
            {
                inDegree[succ]--;
                if (inDegree[succ] == 0) queue.Enqueue(succ);
            }
        }
        // If a cycle exists (shouldn't, given AddWorkItemDependency's no-self-dependency + typical
        // usage), topoOrder will be shorter than itemIds — fall back to whatever was resolved rather
        // than throwing, since critical-path is a best-effort visualization, not a hard invariant.

        var longestPathEndingAt = itemIds.ToDictionary(id => id, id => durationByItem[id]);
        var bestPredecessor = itemIds.ToDictionary(id => id, id => (Guid?)null);

        foreach (var node in topoOrder)
        {
            foreach (var succ in successors[node])
            {
                var candidate = longestPathEndingAt[node] + durationByItem[succ];
                if (candidate > longestPathEndingAt[succ])
                {
                    longestPathEndingAt[succ] = candidate;
                    bestPredecessor[succ] = node;
                }
            }
        }

        var endNode = longestPathEndingAt.OrderByDescending(kv => kv.Value).First().Key;
        var path = new List<Guid>();
        Guid? current = endNode;
        while (current.HasValue)
        {
            path.Add(current.Value);
            current = bestPredecessor[current.Value];
        }
        path.Reverse();

        return new CriticalPathDto(path);
    }
}

public record CreateGanttBaselineCommand(Guid ProjectId, string Name) : IRequest<GanttBaselineDto>;

public class CreateGanttBaselineCommandValidator : AbstractValidator<CreateGanttBaselineCommand>
{
    public CreateGanttBaselineCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateGanttBaselineCommandHandler : IRequestHandler<CreateGanttBaselineCommand, GanttBaselineDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateGanttBaselineCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<GanttBaselineDto> Handle(CreateGanttBaselineCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var workItems = await _db.WorkItems.Where(w => w.ProjectId == request.ProjectId).ToListAsync(cancellationToken);

        var baseline = new GanttBaseline { ProjectId = request.ProjectId, Name = request.Name };
        _db.GanttBaselines.Add(baseline);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var w in workItems)
        {
            _db.GanttBaselineItems.Add(new GanttBaselineItem
            {
                BaselineId = baseline.Id,
                WorkItemId = w.Id,
                Title = w.Title,
                PlannedStartDate = GanttCalculator.EffectiveStart(w),
                PlannedEndDate = GanttCalculator.EffectiveEnd(w)
            });
        }
        await _db.SaveChangesAsync(cancellationToken);

        var withItems = await _db.GanttBaselines.Include(b => b.Items).FirstAsync(b => b.Id == baseline.Id, cancellationToken);
        return _mapper.Map<GanttBaselineDto>(withItems);
    }
}

public record ListGanttBaselinesQuery(Guid ProjectId) : IRequest<IReadOnlyList<GanttBaselineDto>>;

public class ListGanttBaselinesQueryHandler : IRequestHandler<ListGanttBaselinesQuery, IReadOnlyList<GanttBaselineDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListGanttBaselinesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<GanttBaselineDto>> Handle(ListGanttBaselinesQuery request, CancellationToken cancellationToken)
    {
        var baselines = await _db.GanttBaselines.Where(b => b.ProjectId == request.ProjectId)
            .Include(b => b.Items).OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);
        return baselines.Select(b => _mapper.Map<GanttBaselineDto>(b)).ToList();
    }
}

public record GetGanttBaselineQuery(Guid ProjectId, Guid BaselineId) : IRequest<GanttBaselineDetailDto>;

public class GetGanttBaselineQueryHandler : IRequestHandler<GetGanttBaselineQuery, GanttBaselineDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetGanttBaselineQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<GanttBaselineDetailDto> Handle(GetGanttBaselineQuery request, CancellationToken cancellationToken)
    {
        var baseline = await _db.GanttBaselines.Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == request.BaselineId && b.ProjectId == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("GanttBaseline", request.BaselineId);

        var items = baseline.Items.Select(i => _mapper.Map<GanttBaselineItemDto>(i)).ToList();
        return new GanttBaselineDetailDto(baseline.Id, baseline.Name, baseline.CreatedAt, items);
    }
}

public record ExportGanttCsvQuery(Guid ProjectId) : IRequest<string>;

public class ExportGanttCsvQueryHandler : IRequestHandler<ExportGanttCsvQuery, string>
{
    private readonly IProjectFlowDbContext _db;

    public ExportGanttCsvQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<string> Handle(ExportGanttCsvQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var workItems = await _db.WorkItems.Where(w => w.ProjectId == request.ProjectId)
            .Include(w => w.AssigneeUser)
            .OrderBy(w => w.Position)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Title,Status,Priority,StartDate,EndDate,AssigneeName,StoryPoints");
        foreach (var w in workItems)
        {
            var assigneeName = w.AssigneeUser != null ? $"{w.AssigneeUser.FirstName} {w.AssigneeUser.LastName}" : "";
            sb.AppendLine(string.Join(',', new[]
            {
                CsvField(w.Title),
                CsvField(w.Status.ToString()),
                CsvField(w.Priority.ToString()),
                CsvField(GanttCalculator.EffectiveStart(w).ToString("yyyy-MM-dd")),
                CsvField(GanttCalculator.EffectiveEnd(w).ToString("yyyy-MM-dd")),
                CsvField(assigneeName),
                CsvField(w.StoryPoints?.ToString() ?? "")
            }));
        }
        return sb.ToString();
    }

    private static string CsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
