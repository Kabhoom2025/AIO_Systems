using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;

namespace ProjectFlowAI.Application.Features.DocPages;

public record ListDocPagesQuery(Guid OrganizationId, Guid? ProjectId, DocScope? Scope, DocCategory? Category, Guid? ParentPageId)
    : IRequest<IReadOnlyList<DocPageSummaryDto>>;

public class ListDocPagesQueryHandler : IRequestHandler<ListDocPagesQuery, IReadOnlyList<DocPageSummaryDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListDocPagesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<DocPageSummaryDto>> Handle(ListDocPagesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.DocPages.Include(p => p.CreatedByUser).Include(p => p.UpdatedByUser)
            .Where(p => p.OrganizationId == request.OrganizationId && p.ParentPageId == request.ParentPageId);

        if (request.ProjectId.HasValue) query = query.Where(p => p.ProjectId == request.ProjectId.Value);
        if (request.Scope.HasValue) query = query.Where(p => p.Scope == request.Scope.Value);
        if (request.Category.HasValue) query = query.Where(p => p.Category == request.Category.Value);

        var entities = await query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt).ToListAsync(cancellationToken);
        return entities.Select(e => _mapper.Map<DocPageSummaryDto>(e)).ToList();
    }
}

public record GetDocPageQuery(Guid Id) : IRequest<DocPageDetailDto>;

public class GetDocPageQueryHandler : IRequestHandler<GetDocPageQuery, DocPageDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetDocPageQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DocPageDetailDto> Handle(GetDocPageQuery request, CancellationToken cancellationToken) =>
        await DocPageMapping.MapDetailAsync(_db, _mapper, request.Id, cancellationToken);
}

public record ListDocPageVersionsQuery(Guid PageId) : IRequest<IReadOnlyList<DocPageVersionSummaryDto>>;

public class ListDocPageVersionsQueryHandler : IRequestHandler<ListDocPageVersionsQuery, IReadOnlyList<DocPageVersionSummaryDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListDocPageVersionsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<DocPageVersionSummaryDto>> Handle(ListDocPageVersionsQuery request, CancellationToken cancellationToken)
    {
        if (!await _db.DocPages.AnyAsync(p => p.Id == request.PageId, cancellationToken))
            throw new NotFoundException("DocPage", request.PageId);

        var versions = await _db.DocPageVersions.Include(v => v.EditedByUser)
            .Where(v => v.PageId == request.PageId).OrderByDescending(v => v.VersionNumber).ToListAsync(cancellationToken);
        return versions.Select(v => _mapper.Map<DocPageVersionSummaryDto>(v)).ToList();
    }
}

public record GetDocPageVersionQuery(Guid PageId, Guid VersionId) : IRequest<DocPageVersionDetailDto>;

public class GetDocPageVersionQueryHandler : IRequestHandler<GetDocPageVersionQuery, DocPageVersionDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetDocPageVersionQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DocPageVersionDetailDto> Handle(GetDocPageVersionQuery request, CancellationToken cancellationToken)
    {
        var version = await _db.DocPageVersions.Include(v => v.EditedByUser)
            .FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PageId == request.PageId, cancellationToken)
            ?? throw new NotFoundException("DocPageVersion", request.VersionId);
        return _mapper.Map<DocPageVersionDetailDto>(version);
    }
}

public record ListDocPageCommentsQuery(Guid PageId) : IRequest<IReadOnlyList<DocPageCommentDto>>;

public class ListDocPageCommentsQueryHandler : IRequestHandler<ListDocPageCommentsQuery, IReadOnlyList<DocPageCommentDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListDocPageCommentsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<DocPageCommentDto>> Handle(ListDocPageCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _db.DocPageComments.Include(c => c.AuthorUser)
            .Where(c => c.PageId == request.PageId).OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);
        return comments.Select(c => _mapper.Map<DocPageCommentDto>(c)).ToList();
    }
}
