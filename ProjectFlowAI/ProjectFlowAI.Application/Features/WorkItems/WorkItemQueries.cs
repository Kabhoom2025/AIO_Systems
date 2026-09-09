using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Application.Features.WorkItems;

/// <summary>Shared Include chain so every WorkItem query returns enough navigation data for the
/// WorkItemDto computed members (AssigneeFullName/ReporterFullName/Labels) to map correctly.</summary>
internal static class WorkItemIncludes
{
    public static IQueryable<Domain.Entities.WorkItem> WithListNavigations(IQueryable<Domain.Entities.WorkItem> query) =>
        query.Include(w => w.AssigneeUser).Include(w => w.ReporterUser)
            .Include(w => w.WorkItemLabels).ThenInclude(wl => wl.Label);
}

public record ListWorkItemsQuery(Guid ProjectId, WorkItemStatus? Status = null, Guid? AssigneeId = null,
    WorkItemPriority? Priority = null, Guid? LabelId = null, string? Search = null,
    int Page = 1, int PageSize = 20, string? SortBy = null, string? SortDir = "asc") : IRequest<PagedResult<WorkItemDto>>;

public class ListWorkItemsQueryHandler : IRequestHandler<ListWorkItemsQuery, PagedResult<WorkItemDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListWorkItemsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<WorkItemDto>> Handle(ListWorkItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.WorkItems.Where(w => w.ProjectId == request.ProjectId);
        if (request.Status.HasValue) query = query.Where(w => w.Status == request.Status.Value);
        if (request.AssigneeId.HasValue) query = query.Where(w => w.AssigneeUserId == request.AssigneeId.Value);
        if (request.Priority.HasValue) query = query.Where(w => w.Priority == request.Priority.Value);
        if (request.LabelId.HasValue) query = query.Where(w => w.WorkItemLabels.Any(wl => wl.LabelId == request.LabelId.Value));
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(w => w.Title.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Domain.Entities.WorkItem.Position));

        var entities = await WorkItemIncludes.WithListNavigations(sorted)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(e => _mapper.Map<WorkItemDto>(e)).ToList();
        return new PagedResult<WorkItemDto>(items, total, request.Page, request.PageSize);
    }
}

/// <summary>Groups every WorkItem in the project by its Status column, sorted by Position within
/// each column, so the frontend doesn't have to do the grouping itself.</summary>
public record GetKanbanBoardQuery(Guid ProjectId) : IRequest<KanbanBoardDto>;

public class GetKanbanBoardQueryHandler : IRequestHandler<GetKanbanBoardQuery, KanbanBoardDto>
{
    private static readonly WorkItemStatus[] AllStatuses =
    {
        WorkItemStatus.Backlog, WorkItemStatus.ToDo, WorkItemStatus.InProgress,
        WorkItemStatus.CodeReview, WorkItemStatus.Testing, WorkItemStatus.Blocked, WorkItemStatus.Done
    };

    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetKanbanBoardQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<KanbanBoardDto> Handle(GetKanbanBoardQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);

        var entities = await WorkItemIncludes.WithListNavigations(_db.WorkItems.Where(w => w.ProjectId == request.ProjectId))
            .OrderBy(w => w.Position).ToListAsync(cancellationToken);

        var dtosByStatus = entities.Select(e => _mapper.Map<WorkItemDto>(e)).GroupBy(d => d.Status)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<WorkItemDto>)g.OrderBy(d => d.Position).ToList());

        var columns = AllStatuses
            .Select(status => new KanbanColumnDto(status, dtosByStatus.TryGetValue(status, out var items) ? items : new List<WorkItemDto>()))
            .ToList();

        return new KanbanBoardDto(request.ProjectId, columns);
    }
}

public record GetWorkItemQuery(Guid Id) : IRequest<WorkItemDetailDto>;

public class GetWorkItemQueryHandler : IRequestHandler<GetWorkItemQuery, WorkItemDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetWorkItemQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<WorkItemDetailDto> Handle(GetWorkItemQuery request, CancellationToken cancellationToken)
    {
        var workItem = await _db.WorkItems
            .Include(w => w.AssigneeUser).Include(w => w.ReporterUser)
            .Include(w => w.WorkItemLabels).ThenInclude(wl => wl.Label)
            .Include(w => w.ChecklistItems)
            .Include(w => w.Followers)
            .Include(w => w.Dependencies).ThenInclude(d => d.DependsOnWorkItem)
            .Include(w => w.Comments).ThenInclude(c => c.AuthorUser)
            .Include(w => w.Comments).ThenInclude(c => c.Mentions)
            .Include(w => w.Attachments)
            .Include(w => w.Activities)
            .Include(w => w.TimeLogs)
            .Include(w => w.CustomFieldValues)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItem", request.Id);

        return _mapper.Map<WorkItemDetailDto>(workItem);
    }
}

/// <summary>Not exposed as a public DTO — used only by AttachmentsController to resolve the local
/// storage path + display filename before streaming the file back to the client.</summary>
public record AttachmentFileResult(string FileName, string FileUrl);

public record GetWorkItemAttachmentFileQuery(Guid Id) : IRequest<AttachmentFileResult>;

public class GetWorkItemAttachmentFileQueryHandler : IRequestHandler<GetWorkItemAttachmentFileQuery, AttachmentFileResult>
{
    private readonly IProjectFlowDbContext _db;

    public GetWorkItemAttachmentFileQueryHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<AttachmentFileResult> Handle(GetWorkItemAttachmentFileQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _db.WorkItemAttachments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("WorkItemAttachment", request.Id);
        return new AttachmentFileResult(attachment.FileName, attachment.FileUrl);
    }
}
