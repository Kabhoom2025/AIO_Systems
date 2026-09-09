using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Application.Features.Calendar;

/// <summary>Read-only projection over WorkItems/Milestones/Sprints — not a writable resource, per spec.</summary>
public record GetCalendarQuery(Guid OrganizationId, Guid? ProjectId, DateTime From, DateTime To) : IRequest<CalendarResultDto>;

public class GetCalendarQueryHandler : IRequestHandler<GetCalendarQuery, CalendarResultDto>
{
    private readonly IProjectFlowDbContext _db;

    public GetCalendarQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<CalendarResultDto> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var projectsQuery = _db.Projects.Where(p => p.OrganizationId == request.OrganizationId);
        if (request.ProjectId.HasValue) projectsQuery = projectsQuery.Where(p => p.Id == request.ProjectId.Value);

        var projects = await projectsQuery.Select(p => new { p.Id, p.Name }).ToListAsync(cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();
        var projectNameById = projects.ToDictionary(p => p.Id, p => p.Name);

        var events = new List<CalendarEventDto>();

        var dueWorkItems = await _db.WorkItems
            .Where(w => projectIds.Contains(w.ProjectId) && w.DueDate != null && w.DueDate >= request.From && w.DueDate <= request.To)
            .Select(w => new { w.Id, w.Title, w.DueDate, w.ProjectId })
            .ToListAsync(cancellationToken);
        events.AddRange(dueWorkItems.Select(w => new CalendarEventDto(
            w.Id, CalendarEventType.WorkItemDue, w.Title, w.DueDate!.Value, null, w.ProjectId, projectNameById.GetValueOrDefault(w.ProjectId))));

        var dueMilestones = await _db.Milestones
            .Where(m => projectIds.Contains(m.ProjectId) && m.DueDate != null && m.DueDate >= request.From && m.DueDate <= request.To)
            .Select(m => new { m.Id, m.Name, m.DueDate, m.ProjectId })
            .ToListAsync(cancellationToken);
        events.AddRange(dueMilestones.Select(m => new CalendarEventDto(
            m.Id, CalendarEventType.Milestone, m.Name, m.DueDate!.Value, null, m.ProjectId, projectNameById.GetValueOrDefault(m.ProjectId))));

        var sprints = await _db.Sprints
            .Where(s => projectIds.Contains(s.ProjectId)
                && ((s.StartDate >= request.From && s.StartDate <= request.To) || (s.EndDate >= request.From && s.EndDate <= request.To)))
            .Select(s => new { s.Id, s.Name, s.StartDate, s.EndDate, s.ProjectId })
            .ToListAsync(cancellationToken);
        foreach (var s in sprints)
        {
            if (s.StartDate >= request.From && s.StartDate <= request.To)
                events.Add(new CalendarEventDto(s.Id, CalendarEventType.SprintStart, $"{s.Name} starts", s.StartDate, null, s.ProjectId, projectNameById.GetValueOrDefault(s.ProjectId)));
            if (s.EndDate >= request.From && s.EndDate <= request.To)
                events.Add(new CalendarEventDto(s.Id, CalendarEventType.SprintEnd, $"{s.Name} ends", s.EndDate, null, s.ProjectId, projectNameById.GetValueOrDefault(s.ProjectId)));
        }

        return new CalendarResultDto(events.OrderBy(e => e.Date).ToList());
    }
}

public record GetWorkloadQuery(Guid OrganizationId, Guid? ProjectId, DateTime From, DateTime To) : IRequest<WorkloadResultDto>;

/// <summary>Simple resource-calendar aggregate: for every assignee with WorkItems due in range, sum
/// EstimatedHours (default 4h when unset) bucketed by due-date day. Deliberately not a real
/// capacity-planning engine (no working-hours calendar, no leave tracking) — see spec's "don't
/// overbuild" guidance.</summary>
public class GetWorkloadQueryHandler : IRequestHandler<GetWorkloadQuery, WorkloadResultDto>
{
    private const decimal DefaultHoursWhenUnestimated = 4m;

    private readonly IProjectFlowDbContext _db;

    public GetWorkloadQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<WorkloadResultDto> Handle(GetWorkloadQuery request, CancellationToken cancellationToken)
    {
        var projectsQuery = _db.Projects.Where(p => p.OrganizationId == request.OrganizationId);
        if (request.ProjectId.HasValue) projectsQuery = projectsQuery.Where(p => p.Id == request.ProjectId.Value);
        var projectIds = await projectsQuery.Select(p => p.Id).ToListAsync(cancellationToken);

        var dueItems = await _db.WorkItems
            .Where(w => projectIds.Contains(w.ProjectId) && w.AssigneeUserId != null
                && w.DueDate != null && w.DueDate >= request.From && w.DueDate <= request.To)
            .Include(w => w.AssigneeUser)
            .ToListAsync(cancellationToken);

        var rows = dueItems
            .GroupBy(w => (w.AssigneeUserId!.Value, Date: DateOnly.FromDateTime(w.DueDate!.Value)))
            .Select(g => new WorkloadRowDto(
                g.Key.Item1,
                g.First().AssigneeUser != null ? $"{g.First().AssigneeUser!.FirstName} {g.First().AssigneeUser!.LastName}" : "",
                g.Key.Date,
                g.Sum(w => w.EstimatedHours ?? DefaultHoursWhenUnestimated)))
            .OrderBy(r => r.Date).ThenBy(r => r.UserName)
            .ToList();

        return new WorkloadResultDto(rows);
    }
}
