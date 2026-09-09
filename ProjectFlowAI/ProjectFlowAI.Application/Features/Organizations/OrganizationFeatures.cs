using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Organizations;

// ---------------------------------------------------------------------------------------------
// Commands
// ---------------------------------------------------------------------------------------------
public record CreateOrganizationCommand(string Name, string Slug, string? Domain, SubscriptionPlan SubscriptionPlan) : IRequest<OrganizationDto>;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens only.");
        RuleFor(x => x.SubscriptionPlan).IsInEnum();
    }
}

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, OrganizationDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _clock;

    public CreateOrganizationCommandHandler(IProjectFlowDbContext db, IMapper mapper, IDateTimeProvider clock)
    {
        _db = db; _mapper = mapper; _clock = clock;
    }

    public async Task<OrganizationDto> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await _db.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken))
            throw new ConflictException($"An organization with slug '{slug}' already exists.");

        var org = new Organization
        {
            Name = request.Name,
            Slug = slug,
            Domain = request.Domain,
            SubscriptionPlan = request.SubscriptionPlan,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };
        _db.Organizations.Add(org);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrganizationDto>(org);
    }
}

public record UpdateOrganizationCommand(Guid Id, string Name, string? LogoUrl, string? Domain,
    SubscriptionPlan SubscriptionPlan, bool IsActive) : IRequest<OrganizationDto>;

public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.SubscriptionPlan).IsInEnum();
    }
}

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, OrganizationDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _clock;

    public UpdateOrganizationCommandHandler(IProjectFlowDbContext db, IMapper mapper, IDateTimeProvider clock)
    {
        _db = db; _mapper = mapper; _clock = clock;
    }

    public async Task<OrganizationDto> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization", request.Id);

        org.Name = request.Name;
        org.LogoUrl = request.LogoUrl;
        org.Domain = request.Domain;
        org.SubscriptionPlan = request.SubscriptionPlan;
        org.IsActive = request.IsActive;
        org.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrganizationDto>(org);
    }
}

// ---------------------------------------------------------------------------------------------
// Queries
// ---------------------------------------------------------------------------------------------
public record GetOrganizationQuery(Guid Id) : IRequest<OrganizationDto>;

public class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, OrganizationDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetOrganizationQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<OrganizationDto> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization", request.Id);
        return _mapper.Map<OrganizationDto>(org);
    }
}

public record ListOrganizationsQuery(int Page = 1, int PageSize = 20, string? SortBy = null, string? SortDir = "asc",
    string? Search = null, bool? IsActive = null) : IRequest<PagedResult<OrganizationDto>>;

public class ListOrganizationsQueryHandler : IRequestHandler<ListOrganizationsQuery, PagedResult<OrganizationDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListOrganizationsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<OrganizationDto>> Handle(ListOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Organizations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(o => o.Name.Contains(request.Search) || o.Slug.Contains(request.Search));
        if (request.IsActive.HasValue)
            query = query.Where(o => o.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        query = query.ApplySort(request.SortBy, request.SortDir, nameof(Organization.Name));

        var items = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<OrganizationDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<OrganizationDto>(items, total, request.Page, request.PageSize);
    }
}
