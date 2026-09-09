using System.Text.Json;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Features.DocPages;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Ai;

/// <summary>Defensive JSON helpers shared by every AI command handler. The PRIMARY approach to
/// getting clean JSON out of Claude is a strict system-prompt instruction ("return ONLY a JSON
/// array/object, no markdown fences, no prose"); StripFences is only a fallback for the rare case
/// the model doesn't fully comply.</summary>
public static class AiJson
{
    public static string StripFences(string text)
    {
        var t = text.Trim();
        if (!t.StartsWith("```")) return t;

        var firstNewline = t.IndexOf('\n');
        t = firstNewline >= 0 ? t[(firstNewline + 1)..] : t.TrimStart('`');

        var lastFence = t.LastIndexOf("```", StringComparison.Ordinal);
        if (lastFence >= 0) t = t[..lastFence];

        return t.Trim();
    }

    public static T Parse<T>(string text)
    {
        var cleaned = StripFences(text);
        return JsonSerializer.Deserialize<T>(cleaned, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("AI response parsed to null.");
    }
}

// ---------------------------------------------------------------------------
// POST /ai/generate-tasks
// ---------------------------------------------------------------------------

public record GenerateTasksCommand(Guid ProjectId, string Prompt) : IRequest<GenerateTasksResultDto>;

public class GenerateTasksCommandValidator : AbstractValidator<GenerateTasksCommand>
{
    public GenerateTasksCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Prompt).NotEmpty();
    }
}

internal record RawTaskSuggestion(string Title, string? Description, string? Priority, string? Type, int? StoryPoints);

public class GenerateTasksCommandHandler : IRequestHandler<GenerateTasksCommand, GenerateTasksResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public GenerateTasksCommandHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<GenerateTasksResultDto> Handle(GenerateTasksCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var priorities = string.Join(", ", Enum.GetNames<WorkItemPriority>());
        var types = string.Join(", ", Enum.GetNames<WorkItemType>());
        var system = $"""
            You generate task breakdowns for a software project management tool. Given a description
            of work, return ONLY a JSON array (no markdown fences, no prose, no explanation) of task
            suggestions. Each element must be an object with exactly these fields:
            title (string), description (string), priority (one of: {priorities}),
            type (one of: {types}), storyPoints (integer or null).
            Use ONLY the listed priority/type values — they must match exactly (case-sensitive).
            """;

        var raw = await _ai.CompleteAsync(system, request.Prompt, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<List<RawTaskSuggestion>>(raw);

        var suggestions = parsed.Select(p => new TaskSuggestionDto(
            p.Title,
            p.Description ?? string.Empty,
            Enum.TryParse<WorkItemPriority>(p.Priority, ignoreCase: true, out var pr) ? pr : WorkItemPriority.Medium,
            Enum.TryParse<WorkItemType>(p.Type, ignoreCase: true, out var ty) ? ty : WorkItemType.Task,
            p.StoryPoints)).ToList();

        return new GenerateTasksResultDto(suggestions);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/plan-sprint
// ---------------------------------------------------------------------------

public record PlanSprintCommand(Guid ProjectId, string SprintGoal, int CapacityPoints) : IRequest<PlanSprintResultDto>;

public class PlanSprintCommandValidator : AbstractValidator<PlanSprintCommand>
{
    public PlanSprintCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.SprintGoal).NotEmpty();
        RuleFor(x => x.CapacityPoints).GreaterThan(0);
    }
}

internal record PlanSprintAiResponse(List<string> SelectedWorkItemIds, string Reasoning);

public class PlanSprintCommandHandler : IRequestHandler<PlanSprintCommand, PlanSprintResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public PlanSprintCommandHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<PlanSprintResultDto> Handle(PlanSprintCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var backlog = await _db.WorkItems
            .Where(w => w.ProjectId == request.ProjectId && w.Status == WorkItemStatus.Backlog)
            .Select(w => new { w.Id, w.Title, w.StoryPoints, w.Priority })
            .ToListAsync(cancellationToken);

        var backlogJson = JsonSerializer.Serialize(backlog.Select(b => new
        {
            id = b.Id.ToString(),
            title = b.Title,
            storyPoints = b.StoryPoints,
            priority = b.Priority.ToString()
        }));

        var system = """
            You plan sprints for a software project management tool. You will be given a JSON array
            of backlog work items (id, title, storyPoints, priority), a sprint goal, and a capacity
            in story points. Select a subset of the backlog items (by id) that best serves the goal
            while fitting within capacity. Return ONLY a JSON object (no markdown fences, no prose)
            with exactly these fields: selectedWorkItemIds (array of the chosen item id strings),
            reasoning (string explaining the selection).
            """;
        var userMessage = $"Sprint goal: {request.SprintGoal}\nCapacity (story points): {request.CapacityPoints}\nBacklog:\n{backlogJson}";

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<PlanSprintAiResponse>(raw);

        var validIds = backlog.Select(b => b.Id).ToHashSet();
        var selected = parsed.SelectedWorkItemIds
            .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue && validIds.Contains(g.Value))
            .Select(g => g!.Value)
            .ToList();

        return new PlanSprintResultDto(selected, parsed.Reasoning);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/analyze-bug
// ---------------------------------------------------------------------------

public record AnalyzeBugCommand(Guid WorkItemId) : IRequest<AnalyzeBugResultDto>;

public class AnalyzeBugCommandValidator : AbstractValidator<AnalyzeBugCommand>
{
    public AnalyzeBugCommandValidator() => RuleFor(x => x.WorkItemId).NotEmpty();
}

internal record AnalyzeBugAiResponse(string ProbableRootCause, string ReproSteps, string SuggestedSeverity, string Reasoning);

public class AnalyzeBugCommandHandler : IRequestHandler<AnalyzeBugCommand, AnalyzeBugResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public AnalyzeBugCommandHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<AnalyzeBugResultDto> Handle(AnalyzeBugCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();

        var item = await _db.WorkItems.Include(w => w.Comments)
            .FirstOrDefaultAsync(w => w.Id == request.WorkItemId, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.WorkItemId);

        var comments = string.Join("\n---\n", item.Comments.OrderBy(c => c.CreatedAt).Select(c => c.Body));
        var system = """
            You analyze bug reports for a software project management tool. Given a work item's
            title, description, type, and comments, return ONLY a JSON object (no markdown fences,
            no prose) with exactly these fields: probableRootCause (string), reproSteps (string,
            numbered steps as plain text), suggestedSeverity (one of: Info, Warning, Critical),
            reasoning (string). This only strictly makes sense for Bug-type items, but analyze
            whatever content is given even if the type is something else.
            """;
        var userMessage = $"Type: {item.Type}\nTitle: {item.Title}\nDescription: {item.Description}\nComments:\n{comments}";

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.High, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<AnalyzeBugAiResponse>(raw);

        return new AnalyzeBugResultDto(parsed.ProbableRootCause, parsed.ReproSteps, parsed.SuggestedSeverity, parsed.Reasoning);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/summarize-meeting
// ---------------------------------------------------------------------------

public record SummarizeMeetingCommand(string RawNotes, Guid ProjectId, bool SaveAsWikiPage, Guid ActorUserId) : IRequest<SummarizeMeetingResultDto>;

public class SummarizeMeetingCommandValidator : AbstractValidator<SummarizeMeetingCommand>
{
    public SummarizeMeetingCommandValidator()
    {
        RuleFor(x => x.RawNotes).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}

internal record SummarizeMeetingAiResponse(string Summary, List<string> ActionItems);

public class SummarizeMeetingCommandHandler : IRequestHandler<SummarizeMeetingCommand, SummarizeMeetingResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;
    private readonly IMediator _mediator;

    public SummarizeMeetingCommandHandler(IProjectFlowDbContext db, IAiClient ai, IMediator mediator)
    {
        _db = db; _ai = ai; _mediator = mediator;
    }

    public async Task<SummarizeMeetingResultDto> Handle(SummarizeMeetingCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var system = """
            You summarize meeting notes for a software project management tool. Return ONLY a JSON
            object (no markdown fences, no prose) with exactly these fields: summary (string,
            a concise written summary), actionItems (array of strings, one per action item).
            """;

        var raw = await _ai.CompleteAsync(system, request.RawNotes, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<SummarizeMeetingAiResponse>(raw);
        var actionItems = parsed.ActionItems.Select(a => new ActionItemDto(a)).ToList();

        Guid? wikiPageId = null;
        if (request.SaveAsWikiPage)
        {
            var page = await _mediator.Send(new CreateDocPageCommand(
                project.OrganizationId, null, DocScope.OrgWiki, DocCategory.MeetingNotes,
                $"Meeting notes — {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", parsed.Summary, null, request.ActorUserId), cancellationToken);
            wikiPageId = page.Id;
        }

        return new SummarizeMeetingResultDto(parsed.Summary, actionItems, wikiPageId);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/chat, GET /ai/chat/conversations, GET /ai/chat/conversations/{id}/messages
// ---------------------------------------------------------------------------

public record ChatCommand(Guid? ProjectId, string Message, Guid? ConversationId, Guid UserId) : IRequest<ChatResultDto>;

public class ChatCommandValidator : AbstractValidator<ChatCommand>
{
    public ChatCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class ChatCommandHandler : IRequestHandler<ChatCommand, ChatResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public ChatCommandHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<ChatResultDto> Handle(ChatCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();

        AiChatConversation conversation;
        if (request.ConversationId.HasValue)
        {
            conversation = await _db.AiChatConversations.Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value, cancellationToken)
                ?? throw new NotFoundException("AiChatConversation", request.ConversationId.Value);
        }
        else
        {
            conversation = new AiChatConversation
            {
                ProjectId = request.ProjectId,
                UserId = request.UserId,
                Title = request.Message.Length > 80 ? request.Message[..80] : request.Message
            };
            _db.AiChatConversations.Add(conversation);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var systemPrompt = await BuildProjectContextAsync(request.ProjectId, cancellationToken);

        var history = conversation.Messages.OrderBy(m => m.CreatedAt)
            .Select(m => (IsUser: m.Role == AiChatRole.User, m.Content)).ToList();
        history.Add((true, request.Message));

        var reply = await _ai.CompleteConversationAsync(systemPrompt, history, AiEffort.Medium, cancellationToken: cancellationToken);

        _db.AiChatMessages.Add(new AiChatMessage { ConversationId = conversation.Id, Role = AiChatRole.User, Content = request.Message });
        _db.AiChatMessages.Add(new AiChatMessage { ConversationId = conversation.Id, Role = AiChatRole.Assistant, Content = reply });
        await _db.SaveChangesAsync(cancellationToken);

        return new ChatResultDto(conversation.Id, reply);
    }

    private async Task<string> BuildProjectContextAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        const string baseSystem = "You are an AI assistant embedded in a project management tool. Answer contextually about the project described below when relevant.";
        if (!projectId.HasValue) return baseSystem;

        var project = await _db.Projects.Include(p => p.WorkItems).Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == projectId.Value, cancellationToken);
        if (project == null) return baseSystem;

        var counts = project.WorkItems.GroupBy(w => w.Status).Select(g => $"{g.Key}: {g.Count()}");
        var activeSprint = project.Sprints.FirstOrDefault(s => s.Status == SprintStatus.Active);

        return $"""
            {baseSystem}
            Project: {project.Name} (status: {project.Status})
            Open work item counts by status: {string.Join(", ", counts)}
            Active sprint: {(activeSprint != null ? activeSprint.Name : "none")}
            """;
    }
}

public record ListAiChatConversationsQuery(Guid? ProjectId, Guid UserId) : IRequest<IReadOnlyList<AiChatConversationSummaryDto>>;

public class ListAiChatConversationsQueryHandler : IRequestHandler<ListAiChatConversationsQuery, IReadOnlyList<AiChatConversationSummaryDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListAiChatConversationsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<AiChatConversationSummaryDto>> Handle(ListAiChatConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.AiChatConversations.Include(c => c.Messages).Where(c => c.UserId == request.UserId).AsQueryable();
        if (request.ProjectId.HasValue) query = query.Where(c => c.ProjectId == request.ProjectId.Value);

        var entities = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<AiChatConversationSummaryDto>(e)).ToList();
    }
}

public record GetAiChatMessagesQuery(Guid ConversationId) : IRequest<IReadOnlyList<AiChatMessageDto>>;

public class GetAiChatMessagesQueryHandler : IRequestHandler<GetAiChatMessagesQuery, IReadOnlyList<AiChatMessageDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetAiChatMessagesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<AiChatMessageDto>> Handle(GetAiChatMessagesQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.AiChatConversations.AnyAsync(c => c.Id == request.ConversationId, cancellationToken))
            throw new NotFoundException("AiChatConversation", request.ConversationId);

        var entities = await _db.AiChatMessages.Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.CreatedAt).ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<AiChatMessageDto>(e)).ToList();
    }
}

// ---------------------------------------------------------------------------
// GET /ai/risk-prediction
// ---------------------------------------------------------------------------

public record GetRiskPredictionQuery(Guid ProjectId) : IRequest<RiskPredictionDto>;

internal record RiskPredictionAiResponse(string RiskLevel, string Explanation);

public class GetRiskPredictionQueryHandler : IRequestHandler<GetRiskPredictionQuery, RiskPredictionDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public GetRiskPredictionQueryHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<RiskPredictionDto> Handle(GetRiskPredictionQuery request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();

        var project = await _db.Projects.Include(p => p.WorkItems).Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var today = DateTime.UtcNow.Date;
        var overdueCount = project.WorkItems.Count(w => w.Status != WorkItemStatus.Done && w.DueDate != null && w.DueDate.Value.Date < today);
        var blockedCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.Blocked);
        var activeSprint = project.Sprints.FirstOrDefault(s => s.Status == SprintStatus.Active);
        double? sprintProgressPercent = null;
        if (activeSprint != null)
        {
            var items = project.WorkItems.Where(w => w.SprintId == activeSprint.Id).ToList();
            var total = items.Sum(w => w.StoryPoints ?? 0);
            var done = items.Where(w => w.Status == WorkItemStatus.Done).Sum(w => w.StoryPoints ?? 0);
            sprintProgressPercent = total == 0 ? 0 : (double)done / total * 100.0;
        }

        var system = """
            You assess delivery risk for a software project. Given overdue task count, blocked task
            count, and current sprint progress, return ONLY a JSON object (no markdown fences, no
            prose) with exactly these fields: riskLevel (one of: Low, Medium, High), explanation (string).
            """;
        var userMessage = $"Overdue tasks: {overdueCount}\nBlocked tasks: {blockedCount}\nActive sprint progress: {(sprintProgressPercent.HasValue ? $"{sprintProgressPercent:F1}%" : "no active sprint")}";

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.High, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<RiskPredictionAiResponse>(raw);
        return new RiskPredictionDto(parsed.RiskLevel, parsed.Explanation);
    }
}

// ---------------------------------------------------------------------------
// GET /ai/resource-allocation
// ---------------------------------------------------------------------------

public record GetResourceAllocationQuery(Guid ProjectId) : IRequest<ResourceAllocationResultDto>;

internal record ResourceAllocationAiSuggestion(string UserId, string Recommendation);
internal record ResourceAllocationAiResponse(List<ResourceAllocationAiSuggestion> Suggestions, string Reasoning);

public class GetResourceAllocationQueryHandler : IRequestHandler<GetResourceAllocationQuery, ResourceAllocationResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public GetResourceAllocationQueryHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<ResourceAllocationResultDto> Handle(GetResourceAllocationQuery request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var assignedItems = await _db.WorkItems
            .Where(w => w.ProjectId == request.ProjectId && w.AssigneeUserId != null && w.Status != WorkItemStatus.Done)
            .Select(w => new { w.AssigneeUserId, w.EstimatedHours })
            .ToListAsync(cancellationToken);

        var userIds = assignedItems.Select(a => a.AssigneeUserId!.Value).Distinct().ToList();
        var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, cancellationToken);

        var workload = userIds.Select(userId => new
        {
            userId = userId.ToString(),
            userName = users.TryGetValue(userId, out var u) ? $"{u.FirstName} {u.LastName}" : "Unknown",
            taskCount = assignedItems.Count(a => a.AssigneeUserId == userId),
            estimatedHours = assignedItems.Where(a => a.AssigneeUserId == userId).Sum(a => a.EstimatedHours ?? 0m)
        }).ToList();

        var system = """
            You rebalance workload for a software project team. Given each assignee's current open
            task count and estimated hours, return ONLY a JSON object (no markdown fences, no prose)
            with exactly these fields: suggestions (array of objects with userId (string) and
            recommendation (string)), reasoning (string).
            """;
        var userMessage = JsonSerializer.Serialize(workload);

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<ResourceAllocationAiResponse>(raw);

        var suggestions = parsed.Suggestions
            .Select(s => Guid.TryParse(s.UserId, out var uid) && users.TryGetValue(uid, out var user)
                ? new ResourceAllocationSuggestionDto(uid, $"{user.FirstName} {user.LastName}", s.Recommendation)
                : null)
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();

        return new ResourceAllocationResultDto(suggestions, parsed.Reasoning);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/estimate-story-points
// ---------------------------------------------------------------------------

public record EstimateStoryPointsCommand(string Title, string? Description) : IRequest<EstimateStoryPointsResultDto>;

public class EstimateStoryPointsCommandValidator : AbstractValidator<EstimateStoryPointsCommand>
{
    public EstimateStoryPointsCommandValidator() => RuleFor(x => x.Title).NotEmpty();
}

internal record EstimateStoryPointsAiResponse(int SuggestedPoints, string Reasoning);

public class EstimateStoryPointsCommandHandler : IRequestHandler<EstimateStoryPointsCommand, EstimateStoryPointsResultDto>
{
    private static readonly int[] FibonacciPoints = { 1, 2, 3, 5, 8, 13 };
    private readonly IAiClient _ai;

    public EstimateStoryPointsCommandHandler(IAiClient ai) => _ai = ai;

    public async Task<EstimateStoryPointsResultDto> Handle(EstimateStoryPointsCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();

        var system = """
            You estimate story points for software tasks using the Fibonacci-ish scale 1, 2, 3, 5, 8, 13.
            Return ONLY a JSON object (no markdown fences, no prose) with exactly these fields:
            suggestedPoints (integer, MUST be one of 1, 2, 3, 5, 8, 13), reasoning (string).
            """;
        var userMessage = $"Title: {request.Title}\nDescription: {request.Description}";

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<EstimateStoryPointsAiResponse>(raw);

        // Defensive clamp: snap whatever the model returned to the nearest allowed Fibonacci value,
        // since the contract requires a real Fibonacci-ish value even if the model drifts slightly.
        var points = FibonacciPoints.Contains(parsed.SuggestedPoints)
            ? parsed.SuggestedPoints
            : FibonacciPoints.OrderBy(f => Math.Abs(f - parsed.SuggestedPoints)).First();

        return new EstimateStoryPointsResultDto(points, parsed.Reasoning);
    }
}

// ---------------------------------------------------------------------------
// GET /ai/predict-deadline
// ---------------------------------------------------------------------------

public record GetPredictDeadlineQuery(Guid ProjectId) : IRequest<PredictDeadlineResultDto>;

internal record PredictDeadlineAiResponse(string PredictedDate, string Confidence, string Reasoning);

public class GetPredictDeadlineQueryHandler : IRequestHandler<GetPredictDeadlineQuery, PredictDeadlineResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public GetPredictDeadlineQueryHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<PredictDeadlineResultDto> Handle(GetPredictDeadlineQuery request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var remainingPoints = await _db.WorkItems
            .Where(w => w.ProjectId == request.ProjectId && w.Status != WorkItemStatus.Done)
            .SumAsync(w => (int?)w.StoryPoints ?? 0, cancellationToken);

        // Real historical velocity — average completed points across the project's last 10
        // completed sprints (same computation as Phase 3's GetProjectVelocityQueryHandler).
        var completedSprints = await _db.Sprints
            .Where(s => s.ProjectId == request.ProjectId && s.Status == SprintStatus.Completed)
            .Include(s => s.WorkItems)
            .OrderByDescending(s => s.EndDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var velocities = completedSprints.Select(s => s.WorkItems.Where(w => w.Status == WorkItemStatus.Done).Sum(w => w.StoryPoints ?? 0)).ToList();
        var averageVelocity = velocities.Count > 0 ? velocities.Average() : 0d;
        var averageSprintDays = completedSprints.Count > 0
            ? completedSprints.Average(s => Math.Max(1, (s.EndDate - s.StartDate).Days))
            : 14d;

        var system = """
            You predict a project completion deadline for a software project. Given remaining
            backlog story points and historical velocity (points completed per sprint, and typical
            sprint length in days), return ONLY a JSON object (no markdown fences, no prose) with
            exactly these fields: predictedDate (string, ISO 8601 date, e.g. "2026-09-15"),
            confidence (one of: Low, Medium, High), reasoning (string). Ground the prediction in the
            given throughput data rather than guessing.
            """;
        var userMessage = $"Remaining story points: {remainingPoints}\nAverage velocity (points/sprint): {averageVelocity:F1}\n" +
            $"Average sprint length (days): {averageSprintDays:F0}\nCompleted sprints available: {completedSprints.Count}\nToday's date (UTC): {DateTime.UtcNow:yyyy-MM-dd}";

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<PredictDeadlineAiResponse>(raw);

        // Fallback if the model's date doesn't parse: derive one from the same throughput math we
        // gave it, rather than surfacing a broken response.
        DateTime predictedDate;
        if (!DateTime.TryParse(parsed.PredictedDate, out predictedDate))
        {
            var sprintsNeeded = averageVelocity > 0 ? Math.Ceiling(remainingPoints / averageVelocity) : 4;
            predictedDate = DateTime.UtcNow.Date.AddDays(sprintsNeeded * averageSprintDays);
        }

        return new PredictDeadlineResultDto(predictedDate, parsed.Confidence, parsed.Reasoning);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/prioritize-tasks
// ---------------------------------------------------------------------------

public record PrioritizeTasksCommand(Guid ProjectId) : IRequest<PrioritizeTasksResultDto>;

public class PrioritizeTasksCommandValidator : AbstractValidator<PrioritizeTasksCommand>
{
    public PrioritizeTasksCommandValidator() => RuleFor(x => x.ProjectId).NotEmpty();
}

internal record PrioritizedTaskAiItem(string WorkItemId, string SuggestedPriority, string Reasoning);

public class PrioritizeTasksCommandHandler : IRequestHandler<PrioritizeTasksCommand, PrioritizeTasksResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;

    public PrioritizeTasksCommandHandler(IProjectFlowDbContext db, IAiClient ai) { _db = db; _ai = ai; }

    public async Task<PrioritizeTasksResultDto> Handle(PrioritizeTasksCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var items = await _db.WorkItems
            .Where(w => w.ProjectId == request.ProjectId && w.Status != WorkItemStatus.Done)
            .Select(w => new { w.Id, w.Title, Priority = w.Priority.ToString(), w.DueDate })
            .ToListAsync(cancellationToken);

        var system = """
            You re-prioritize a project's backlog/todo work items. Given a JSON array of items (id,
            title, current priority, dueDate), return ONLY a JSON array (no markdown fences, no
            prose) of objects with exactly these fields: workItemId (string), suggestedPriority
            (one of: Lowest, Low, Medium, High, Highest), reasoning (string).
            """;
        var userMessage = JsonSerializer.Serialize(items.Select(i => new { id = i.Id.ToString(), i.Title, priority = i.Priority, dueDate = i.DueDate }));

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<List<PrioritizedTaskAiItem>>(raw);

        var validIds = items.Select(i => i.Id).ToHashSet();
        var results = parsed
            .Where(p => Guid.TryParse(p.WorkItemId, out var g) && validIds.Contains(g))
            .Select(p => new PrioritizedTaskDto(Guid.Parse(p.WorkItemId), p.SuggestedPriority, p.Reasoning))
            .ToList();

        return new PrioritizeTasksResultDto(results);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/review-code
// ---------------------------------------------------------------------------

public record ReviewCodeCommand(string Code, string Language) : IRequest<ReviewCodeResultDto>;

public class ReviewCodeCommandValidator : AbstractValidator<ReviewCodeCommand>
{
    public ReviewCodeCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Language).NotEmpty();
    }
}

internal record CodeReviewAiSuggestion(int? Line, string Comment, string Severity);
internal record ReviewCodeAiResponse(List<CodeReviewAiSuggestion> Suggestions, string Summary);

public class ReviewCodeCommandHandler : IRequestHandler<ReviewCodeCommand, ReviewCodeResultDto>
{
    private readonly IAiClient _ai;

    public ReviewCodeCommandHandler(IAiClient ai) => _ai = ai;

    public async Task<ReviewCodeResultDto> Handle(ReviewCodeCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();

        var system = """
            You are a senior code reviewer. Return ONLY a JSON object (no markdown fences, no prose)
            with exactly these fields: suggestions (array of objects with line (integer or null),
            comment (string), severity (one of: Info, Warning, Critical)), summary (string).
            """;
        var userMessage = $"Language: {request.Language}\n\n```{request.Language}\n{request.Code}\n```";

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.High, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<ReviewCodeAiResponse>(raw);

        var suggestions = parsed.Suggestions.Select(s => new CodeReviewSuggestionDto(s.Line, s.Comment, s.Severity)).ToList();
        return new ReviewCodeResultDto(suggestions, parsed.Summary);
    }
}

// ---------------------------------------------------------------------------
// POST /ai/generate-release-notes
// ---------------------------------------------------------------------------

public record GenerateReleaseNotesCommand(Guid ProjectId, DateTime From, DateTime To, bool SaveAsWikiPage, Guid ActorUserId) : IRequest<GenerateReleaseNotesResultDto>;

public class GenerateReleaseNotesCommandValidator : AbstractValidator<GenerateReleaseNotesCommand>
{
    public GenerateReleaseNotesCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From);
    }
}

public class GenerateReleaseNotesCommandHandler : IRequestHandler<GenerateReleaseNotesCommand, GenerateReleaseNotesResultDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IAiClient _ai;
    private readonly IMediator _mediator;

    public GenerateReleaseNotesCommandHandler(IProjectFlowDbContext db, IAiClient ai, IMediator mediator)
    {
        _db = db; _ai = ai; _mediator = mediator;
    }

    public async Task<GenerateReleaseNotesResultDto> Handle(GenerateReleaseNotesCommand request, CancellationToken cancellationToken)
    {
        if (!_ai.IsConfigured) throw new AiNotConfiguredException();
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var doneItems = await _db.WorkItems
            .Where(w => w.ProjectId == request.ProjectId && w.Status == WorkItemStatus.Done
                && w.UpdatedAt != null && w.UpdatedAt >= request.From && w.UpdatedAt <= request.To)
            .Select(w => new { w.Title, Type = w.Type.ToString() })
            .ToListAsync(cancellationToken);

        var grouped = doneItems.GroupBy(i => i.Type switch
        {
            nameof(WorkItemType.Bug) => "Bug Fixes",
            nameof(WorkItemType.Story) or nameof(WorkItemType.Epic) => "Features",
            _ => "Other"
        }).ToDictionary(g => g.Key, g => g.Select(i => i.Title).ToList());

        var system = """
            You write structured release notes in Markdown for a software project. Given work items
            done in a date range, grouped by Features/Bug Fixes/Other, return ONLY a JSON object
            (no markdown fences around the JSON itself, no prose outside the JSON) with exactly one
            field: markdown (a string containing well-structured Markdown release notes with
            headings per group and a bullet per item).
            """;
        var userMessage = JsonSerializer.Serialize(new { from = request.From, to = request.To, groups = grouped });

        var raw = await _ai.CompleteAsync(system, userMessage, AiEffort.Medium, cancellationToken: cancellationToken);
        var parsed = AiJson.Parse<Dictionary<string, string>>(raw);
        var markdown = parsed.TryGetValue("markdown", out var md) ? md : raw;

        Guid? wikiPageId = null;
        if (request.SaveAsWikiPage)
        {
            var page = await _mediator.Send(new CreateDocPageCommand(
                project.OrganizationId, null, DocScope.OrgWiki, DocCategory.ReleaseNotes,
                $"Release notes — {request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}", markdown, null, request.ActorUserId), cancellationToken);
            wikiPageId = page.Id;
        }

        return new GenerateReleaseNotesResultDto(markdown, wikiPageId);
    }
}
