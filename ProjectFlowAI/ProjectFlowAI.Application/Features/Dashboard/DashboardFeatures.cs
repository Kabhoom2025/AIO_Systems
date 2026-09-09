using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.Notifications;
using ProjectFlowAI.Application.Features.WorkItems;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Application.Features.Dashboard;

/// <summary>Single consolidated "my dashboard" query, always scoped to the calling user via
/// ICurrentUserService (see DashboardController) rather than any client-supplied user id. Pulls
/// together today's/overdue tasks, recent activity across the user's projects, active sprint
/// progress, and unread notification count into one round-trip so the frontend doesn't have to
/// stitch together 5 separate calls on first paint.</summary>
public record GetDashboardQuery(Guid UserId) : IRequest<DashboardDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetDashboardQueryHandler(IProjectFlowDbContext db, IMapper mapper, IMediator mediator)
    {
        _db = db;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var myOpenItemsQuery = WorkItemIncludes.WithListNavigations(
            _db.WorkItems.Where(w => w.AssigneeUserId == request.UserId && w.Status != WorkItemStatus.Done));

        var assignedTaskCount = await myOpenItemsQuery.CountAsync(cancellationToken);

        var todaysTasksEntities = await myOpenItemsQuery
            .Where(w => w.DueDate != null && w.DueDate >= today && w.DueDate < tomorrow)
            .OrderBy(w => w.Position).ToListAsync(cancellationToken);
        var todaysTasks = todaysTasksEntities.Select(e => _mapper.Map<WorkItemDto>(e)).ToList();

        var overdueTasksEntities = await myOpenItemsQuery
            .Where(w => w.DueDate != null && w.DueDate < today)
            .OrderBy(w => w.DueDate).ToListAsync(cancellationToken);
        var overdueTasks = overdueTasksEntities.Select(e => _mapper.Map<WorkItemDto>(e)).ToList();

        // Projects the current user is a member of drive both the recent-activity feed and the
        // active-sprint summaries below — a user only sees activity/sprints for work they're on.
        var myProjectIds = await _db.ProjectMembers
            .Where(pm => pm.UserId == request.UserId)
            .Select(pm => pm.ProjectId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var recentActivity = await BuildRecentActivityAsync(myProjectIds, cancellationToken);
        var activeSprints = await BuildActiveSprintsAsync(myProjectIds, today, cancellationToken);

        // Reuses the exact same handler Phase 4's GET /notifications/unread-count calls, rather
        // than duplicating the "count unread notifications" query here.
        var unreadCount = await _mediator.Send(new GetUnreadNotificationCountQuery(request.UserId), cancellationToken);

        return new DashboardDto(todaysTasks, overdueTasks, assignedTaskCount, recentActivity, activeSprints, unreadCount.Count);
    }

    private async Task<List<RecentActivityItemDto>> BuildRecentActivityAsync(List<Guid> projectIds, CancellationToken cancellationToken)
    {
        if (projectIds.Count == 0) return new List<RecentActivityItemDto>();

        var activities = await _db.WorkItemActivities
            .Where(a => projectIds.Contains(a.WorkItem!.ProjectId))
            .Include(a => a.User)
            .Include(a => a.WorkItem)
            .OrderByDescending(a => a.CreatedAt)
            .Take(15)
            .ToListAsync(cancellationToken);

        return activities.Select(a =>
        {
            var userName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Someone";
            var title = a.WorkItem?.Title ?? "a work item";
            var message = a.Action switch
            {
                "StatusChanged" => $"{userName} moved \"{title}\" from {a.OldValue} to {a.NewValue}",
                "PriorityChanged" => $"{userName} changed the priority of \"{title}\" from {a.OldValue} to {a.NewValue}",
                "Assigned" => $"{userName} reassigned \"{title}\"",
                "Created" => $"{userName} created \"{title}\"",
                "Commented" => $"{userName} commented on \"{title}\"",
                "AttachmentAdded" => $"{userName} attached {a.NewValue} to \"{title}\"",
                "TimeLogged" => $"{userName} logged {a.NewValue} minutes on \"{title}\"",
                _ => $"{userName} updated \"{title}\""
            };
            var linkUrl = a.WorkItem != null ? $"/projects/{a.WorkItem.ProjectId}/tasks/{a.WorkItemId}" : null;
            return new RecentActivityItemDto(a.Id, message, linkUrl, a.CreatedAt);
        }).ToList();
    }

    private async Task<List<ActiveSprintSummaryDto>> BuildActiveSprintsAsync(List<Guid> projectIds, DateTime today, CancellationToken cancellationToken)
    {
        if (projectIds.Count == 0) return new List<ActiveSprintSummaryDto>();

        var sprints = await _db.Sprints
            .Where(s => projectIds.Contains(s.ProjectId) && s.Status == SprintStatus.Active)
            .Include(s => s.WorkItems)
            .Include(s => s.Project)
            .ToListAsync(cancellationToken);

        return sprints.Select(s =>
        {
            var totalPoints = s.WorkItems.Sum(w => w.StoryPoints ?? 0);
            var completedPoints = s.WorkItems.Where(w => w.Status == WorkItemStatus.Done).Sum(w => w.StoryPoints ?? 0);
            var progressPercent = totalPoints == 0 ? 0.0 : (double)completedPoints / totalPoints * 100.0;
            var daysRemaining = Math.Max(0, (s.EndDate.Date - today).Days);
            return new ActiveSprintSummaryDto(s.Id, s.Name, s.ProjectId, s.Project?.Name ?? string.Empty, progressPercent, daysRemaining);
        }).ToList();
    }
}
