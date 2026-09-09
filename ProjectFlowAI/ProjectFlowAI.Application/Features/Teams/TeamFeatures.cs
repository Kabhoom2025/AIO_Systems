using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Teams;

public record CreateTeamCommand(Guid OrganizationId, Guid? DepartmentId, string Name, string? Description) : IRequest<TeamDto>;

public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, TeamDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateTeamCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<TeamDto> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Organizations.AnyAsync(o => o.Id == request.OrganizationId, cancellationToken))
            throw new NotFoundException("Organization", request.OrganizationId);

        var team = new Team
        {
            OrganizationId = request.OrganizationId,
            DepartmentId = request.DepartmentId,
            Name = request.Name,
            Description = request.Description
        };
        _db.Teams.Add(team);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TeamDto>(team);
    }
}

public record UpdateTeamCommand(Guid Id, string Name, string? Description, Guid? DepartmentId) : IRequest<TeamDto>;

public class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, TeamDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public UpdateTeamCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<TeamDto> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Team", request.Id);

        team.Name = request.Name;
        team.Description = request.Description;
        team.DepartmentId = request.DepartmentId;
        team.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TeamDto>(team);
    }
}

public record AddTeamMemberCommand(Guid TeamId, Guid UserId, string RoleInTeam) : IRequest<TeamMemberDto>;

public class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleInTeam).NotEmpty();
    }
}

public class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, TeamMemberDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public AddTeamMemberCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<TeamMemberDto> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Teams.AnyAsync(t => t.Id == request.TeamId, cancellationToken))
            throw new NotFoundException("Team", request.TeamId);
        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User", request.UserId);
        if (await _db.TeamMembers.AnyAsync(m => m.TeamId == request.TeamId && m.UserId == request.UserId, cancellationToken))
            throw new ConflictException("User is already a member of this team.");

        var member = new TeamMember { TeamId = request.TeamId, UserId = request.UserId, RoleInTeam = request.RoleInTeam };
        _db.TeamMembers.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        var withUser = await _db.TeamMembers.Include(m => m.User).FirstAsync(m => m.Id == member.Id, cancellationToken);
        return _mapper.Map<TeamMemberDto>(withUser);
    }
}

public record RemoveTeamMemberCommand(Guid TeamId, Guid UserId) : IRequest<Unit>;

public class RemoveTeamMemberCommandValidator : AbstractValidator<RemoveTeamMemberCommand>
{
    public RemoveTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class RemoveTeamMemberCommandHandler : IRequestHandler<RemoveTeamMemberCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RemoveTeamMemberCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.TeamMembers.FirstOrDefaultAsync(m => m.TeamId == request.TeamId && m.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("TeamMember", request.UserId);

        _db.TeamMembers.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record ListTeamsQuery(Guid OrganizationId, Guid? DepartmentId = null, int Page = 1, int PageSize = 20,
    string? SortBy = null, string? SortDir = "asc", string? Search = null) : IRequest<PagedResult<TeamDto>>;

public class ListTeamsQueryHandler : IRequestHandler<ListTeamsQuery, PagedResult<TeamDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListTeamsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<TeamDto>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Teams.Include(t => t.Members).Where(t => t.OrganizationId == request.OrganizationId);
        if (request.DepartmentId.HasValue)
            query = query.Where(t => t.DepartmentId == request.DepartmentId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Team.Name));

        var items = await sorted.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<TeamDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<TeamDto>(items, total, request.Page, request.PageSize);
    }
}

public record GetTeamQuery(Guid Id) : IRequest<TeamDto>;

public class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, TeamDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public GetTeamQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<TeamDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await _db.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Team", request.Id);
        return _mapper.Map<TeamDto>(team);
    }
}
