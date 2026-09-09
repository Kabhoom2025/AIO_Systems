using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.DocPages;

internal static class DocPageMapping
{
    public static async Task<DocPageDetailDto> MapDetailAsync(IProjectFlowDbContext db, IMapper mapper, Guid pageId, CancellationToken ct)
    {
        var withNav = await db.DocPages
            .Include(p => p.CreatedByUser).Include(p => p.UpdatedByUser).Include(p => p.Versions)
            .FirstAsync(p => p.Id == pageId, ct);
        return mapper.Map<DocPageDetailDto>(withNav);
    }
}

// ---------------------------------------------------------------------------
// CreateDocPage / UpdateDocPage / DeleteDocPage
// ---------------------------------------------------------------------------

public record CreateDocPageCommand(Guid OrganizationId, Guid? ProjectId, DocScope Scope, DocCategory? Category,
    string Title, string Content, Guid? ParentPageId, Guid CreatedByUserId) : IRequest<DocPageDetailDto>;

public class CreateDocPageCommandValidator : AbstractValidator<CreateDocPageCommand>
{
    public CreateDocPageCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty().When(x => x.Scope == DocScope.ProjectDocument)
            .WithMessage("ProjectId is required when Scope is ProjectDocument.");
    }
}

public class CreateDocPageCommandHandler : IRequestHandler<CreateDocPageCommand, DocPageDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateDocPageCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DocPageDetailDto> Handle(CreateDocPageCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);
        if (request.ParentPageId.HasValue && !await _db.DocPages.AnyAsync(p => p.Id == request.ParentPageId.Value, cancellationToken))
            throw new NotFoundException("DocPage", request.ParentPageId.Value);

        var page = new DocPage
        {
            OrganizationId = request.OrganizationId,
            ProjectId = request.ProjectId,
            Scope = request.Scope,
            Category = request.Scope == DocScope.OrgWiki ? request.Category : null,
            Title = request.Title,
            Content = request.Content,
            ParentPageId = request.ParentPageId,
            CreatedByUserId = request.CreatedByUserId
        };
        _db.DocPages.Add(page);
        await _db.SaveChangesAsync(cancellationToken);

        return await DocPageMapping.MapDetailAsync(_db, _mapper, page.Id, cancellationToken);
    }
}

public record UpdateDocPageCommand(Guid Id, string Title, string Content, Guid? ParentPageId,
    DocCategory? Category, Guid ActorUserId) : IRequest<DocPageDetailDto>;

public class UpdateDocPageCommandValidator : AbstractValidator<UpdateDocPageCommand>
{
    public UpdateDocPageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class UpdateDocPageCommandHandler : IRequestHandler<UpdateDocPageCommand, DocPageDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateDocPageCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DocPageDetailDto> Handle(UpdateDocPageCommand request, CancellationToken cancellationToken)
    {
        var page = await _db.DocPages.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("DocPage", request.Id);

        if (request.ParentPageId.HasValue)
        {
            if (request.ParentPageId.Value == page.Id)
                throw new ConflictException("A page cannot be its own parent.");
            if (!await _db.DocPages.AnyAsync(p => p.Id == request.ParentPageId.Value, cancellationToken))
                throw new NotFoundException("DocPage", request.ParentPageId.Value);
        }

        // Snapshot the OLD content as a new version BEFORE applying the update — history is
        // append-only. Exactly one new version row is created per edit; see DocPageVersioningTests.
        var nextVersionNumber = 1 + await _db.DocPageVersions.Where(v => v.PageId == page.Id)
            .Select(v => (int?)v.VersionNumber).MaxAsync(cancellationToken) ?? 1;
        _db.DocPageVersions.Add(new DocPageVersion
        {
            PageId = page.Id,
            Content = page.Content,
            VersionNumber = nextVersionNumber,
            EditedByUserId = request.ActorUserId
        });

        page.Title = request.Title;
        page.Content = request.Content;
        page.ParentPageId = request.ParentPageId;
        page.Category = page.Scope == DocScope.OrgWiki ? request.Category : null;
        page.UpdatedByUserId = request.ActorUserId;
        page.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return await DocPageMapping.MapDetailAsync(_db, _mapper, page.Id, cancellationToken);
    }
}

public record DeleteDocPageCommand(Guid Id) : IRequest<Unit>;

public class DeleteDocPageCommandValidator : AbstractValidator<DeleteDocPageCommand>
{
    public DeleteDocPageCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteDocPageCommandHandler : IRequestHandler<DeleteDocPageCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteDocPageCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteDocPageCommand request, CancellationToken cancellationToken)
    {
        var page = await _db.DocPages.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("DocPage", request.Id);

        // Child pages are re-parented up one level rather than being cascade-deleted, so a
        // deleted parent never silently wipes out its whole subtree.
        var children = await _db.DocPages.Where(p => p.ParentPageId == page.Id).ToListAsync(cancellationToken);
        foreach (var child in children) child.ParentPageId = page.ParentPageId;

        _db.DocPages.Remove(page);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

// ---------------------------------------------------------------------------
// Version restore
// ---------------------------------------------------------------------------

public record RestoreDocPageVersionCommand(Guid PageId, Guid VersionId, Guid ActorUserId) : IRequest<DocPageDetailDto>;

public class RestoreDocPageVersionCommandValidator : AbstractValidator<RestoreDocPageVersionCommand>
{
    public RestoreDocPageVersionCommandValidator()
    {
        RuleFor(x => x.PageId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty();
    }
}

public class RestoreDocPageVersionCommandHandler : IRequestHandler<RestoreDocPageVersionCommand, DocPageDetailDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public RestoreDocPageVersionCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DocPageDetailDto> Handle(RestoreDocPageVersionCommand request, CancellationToken cancellationToken)
    {
        var page = await _db.DocPages.FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken)
            ?? throw new NotFoundException("DocPage", request.PageId);
        var version = await _db.DocPageVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId && v.PageId == request.PageId, cancellationToken)
            ?? throw new NotFoundException("DocPageVersion", request.VersionId);

        // Restoring is itself undoable: snapshot the CURRENT content as a new version first...
        var nextVersionNumber = 1 + await _db.DocPageVersions.Where(v => v.PageId == page.Id)
            .Select(v => (int?)v.VersionNumber).MaxAsync(cancellationToken) ?? 1;
        _db.DocPageVersions.Add(new DocPageVersion
        {
            PageId = page.Id,
            Content = page.Content,
            VersionNumber = nextVersionNumber,
            EditedByUserId = request.ActorUserId
        });

        // ...then overwrite current content with the historical version's content.
        page.Content = version.Content;
        page.UpdatedByUserId = request.ActorUserId;
        page.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return await DocPageMapping.MapDetailAsync(_db, _mapper, page.Id, cancellationToken);
    }
}

// ---------------------------------------------------------------------------
// Comments
// ---------------------------------------------------------------------------

public record AddDocPageCommentCommand(Guid PageId, Guid AuthorUserId, string Body) : IRequest<DocPageCommentDto>;

public class AddDocPageCommentCommandValidator : AbstractValidator<AddDocPageCommentCommand>
{
    public AddDocPageCommentCommandValidator()
    {
        RuleFor(x => x.PageId).NotEmpty();
        RuleFor(x => x.AuthorUserId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}

public class AddDocPageCommentCommandHandler : IRequestHandler<AddDocPageCommentCommand, DocPageCommentDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddDocPageCommentCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<DocPageCommentDto> Handle(AddDocPageCommentCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.DocPages.AnyAsync(p => p.Id == request.PageId, cancellationToken))
            throw new NotFoundException("DocPage", request.PageId);

        var comment = new DocPageComment { PageId = request.PageId, AuthorUserId = request.AuthorUserId, Body = request.Body };
        _db.DocPageComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        var withNav = await _db.DocPageComments.Include(c => c.AuthorUser).FirstAsync(c => c.Id == comment.Id, cancellationToken);
        return _mapper.Map<DocPageCommentDto>(withNav);
    }
}

public record DeleteDocPageCommentCommand(Guid Id) : IRequest<Unit>;

public class DeleteDocPageCommentCommandValidator : AbstractValidator<DeleteDocPageCommentCommand>
{
    public DeleteDocPageCommentCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteDocPageCommentCommandHandler : IRequestHandler<DeleteDocPageCommentCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public DeleteDocPageCommentCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteDocPageCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _db.DocPageComments.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("DocPageComment", request.Id);
        _db.DocPageComments.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
