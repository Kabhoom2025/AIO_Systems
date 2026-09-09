using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Projects;

public record CreateProjectCommand(Guid OrganizationId, string Key, string Name, string? Description,
    Guid OwnerUserId, DateTime? StartDate, DateTime? EndDate) : IRequest<ProjectDto>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Key).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.OwnerUserId, cancellationToken))
            throw new NotFoundException("User", request.OwnerUserId);

        var key = request.Key.Trim().ToUpperInvariant();
        if (await _db.Projects.AnyAsync(p => p.OrganizationId == request.OrganizationId && p.Key == key, cancellationToken))
            throw new ConflictException($"A project with key '{key}' already exists in this organization.");

        var project = new Project
        {
            OrganizationId = request.OrganizationId,
            Key = key,
            Name = request.Name,
            Description = request.Description,
            OwnerUserId = request.OwnerUserId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ProjectStatus.Planning
        };
        _db.Projects.Add(project);
        // Owner is automatically a project member with the "Lead" role.
        _db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = request.OwnerUserId, RoleInProject = "Lead" });
        await _db.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProjectDto>(project);
    }
}

public record UpdateProjectCommand(Guid Id, string Name, string? Description, ProjectStatus Status,
    DateTime? StartDate, DateTime? EndDate, Guid OwnerUserId) : IRequest<ProjectDto>;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerUserId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Project", request.Id);
        if (!await _db.Users.AnyAsync(u => u.Id == request.OwnerUserId, cancellationToken))
            throw new NotFoundException("User", request.OwnerUserId);

        project.Name = request.Name;
        project.Description = request.Description;
        project.Status = request.Status;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.OwnerUserId = request.OwnerUserId;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var withCounts = await _db.Projects.Include(p => p.Members).Include(p => p.WorkItems)
            .FirstAsync(p => p.Id == project.Id, cancellationToken);
        return _mapper.Map<ProjectDto>(withCounts);
    }
}

public record ArchiveProjectCommand(Guid Id) : IRequest<Unit>;

public class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
{
    public ArchiveProjectCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public ArchiveProjectCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Project", request.Id);

        project.IsArchived = true;
        project.Status = ProjectStatus.Archived;
        project.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record AddProjectMemberCommand(Guid ProjectId, Guid UserId, string RoleInProject) : IRequest<ProjectMemberDto>;

public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleInProject).NotEmpty().MaximumLength(100);
    }
}

public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, ProjectMemberDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddProjectMemberCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ProjectMemberDto> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken))
            throw new NotFoundException("Project", request.ProjectId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User", request.UserId);
        if (await _db.ProjectMembers.AnyAsync(m => m.ProjectId == request.ProjectId && m.UserId == request.UserId, cancellationToken))
            throw new ConflictException("User is already a member of this project.");

        var member = new ProjectMember { ProjectId = request.ProjectId, UserId = request.UserId, RoleInProject = request.RoleInProject };
        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        var withUser = await _db.ProjectMembers.Include(m => m.User).FirstAsync(m => m.Id == member.Id, cancellationToken);
        return _mapper.Map<ProjectMemberDto>(withUser);
    }
}

public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest<Unit>;

public class RemoveProjectMemberCommandValidator : AbstractValidator<RemoveProjectMemberCommand>
{
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RemoveProjectMemberCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.ProjectMembers.FirstOrDefaultAsync(
            m => m.ProjectId == request.ProjectId && m.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("ProjectMember", request.UserId);

        _db.ProjectMembers.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record ListProjectsQuery(Guid OrganizationId, ProjectStatus? Status = null, string? Search = null,
    int Page = 1, int PageSize = 20, string? SortBy = null, string? SortDir = "asc") : IRequest<PagedResult<ProjectDto>>;

public class ListProjectsQueryHandler : IRequestHandler<ListProjectsQuery, PagedResult<ProjectDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListProjectsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<ProjectDto>> Handle(ListProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Projects.Where(p => p.OrganizationId == request.OrganizationId);
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.Key.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Project.CreatedAt));

        // Materialize first, then map in memory: the computed WorkItemCounts-by-status grouping is
        // simpler and safer to compute in-process than to push through AutoMapper's ProjectTo/SQL translation.
        var entities = await sorted.Include(p => p.Members).Include(p => p.WorkItems)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(e => _mapper.Map<ProjectDto>(e)).ToList();
        return new PagedResult<ProjectDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetProjectQuery(Guid Id) : IRequest<ProjectDto>;

public class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, ProjectDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetProjectQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<ProjectDto> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.Include(p => p.Members).Include(p => p.WorkItems)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Project", request.Id);
        return _mapper.Map<ProjectDto>(project);
    }
}

public record ListProjectMembersQuery(Guid ProjectId) : IRequest<IReadOnlyList<ProjectMemberDto>>;

public class ListProjectMembersQueryHandler : IRequestHandler<ListProjectMembersQuery, IReadOnlyList<ProjectMemberDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListProjectMembersQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<ProjectMemberDto>> Handle(ListProjectMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _db.ProjectMembers.Include(m => m.User)
            .Where(m => m.ProjectId == request.ProjectId).ToListAsync(cancellationToken);
        return members.Select(m => _mapper.Map<ProjectMemberDto>(m)).ToList();
    }
}
