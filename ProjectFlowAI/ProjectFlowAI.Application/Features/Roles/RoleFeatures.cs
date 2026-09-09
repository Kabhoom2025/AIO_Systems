using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Roles;

public record CreateRoleCommand(Guid? OrganizationId, string Name, IReadOnlyList<string> PermissionKeys) : IRequest<RoleDto>;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator() => RuleFor(x => x.Name).NotEmpty();
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public CreateRoleCommandHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role { OrganizationId = request.OrganizationId, Name = request.Name, IsSystemRole = false };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);

        if (request.PermissionKeys.Count > 0)
        {
            var permissionIds = await _db.Permissions
                .Where(p => request.PermissionKeys.Contains(p.Key))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
            foreach (var permissionId in permissionIds)
                _db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
            await _db.SaveChangesAsync(cancellationToken);
        }

        var withPermissions = await _db.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == role.Id, cancellationToken);
        return _mapper.Map<RoleDto>(withPermissions);
    }
}

public record AssignRoleCommand(Guid UserId, Guid RoleId, Guid? OrganizationId) : IRequest<Unit>;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public AssignRoleCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken))
            throw new NotFoundException("User", request.UserId);
        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken))
            throw new NotFoundException("Role", request.RoleId);

        var already = await _db.UserRoles.AnyAsync(
            ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        if (!already)
        {
            _db.UserRoles.Add(new UserRole { UserId = request.UserId, RoleId = request.RoleId, OrganizationId = request.OrganizationId });
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

public record RevokeRoleCommand(Guid UserId, Guid RoleId) : IRequest<Unit>;

public class RevokeRoleCommandValidator : AbstractValidator<RevokeRoleCommand>
{
    public RevokeRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class RevokeRoleCommandHandler : IRequestHandler<RevokeRoleCommand, Unit>
{
    private readonly IProjectFlowDbContext _db;

    public RevokeRoleCommandHandler(IProjectFlowDbContext db) => _db = db;

    public async Task<Unit> Handle(RevokeRoleCommand request, CancellationToken cancellationToken)
    {
        var userRole = await _db.UserRoles.FirstOrDefaultAsync(
            ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);
        if (userRole != null)
        {
            _db.UserRoles.Remove(userRole);
            await _db.SaveChangesAsync(cancellationToken);
        }
        return Unit.Value;
    }
}

public record ListRolesQuery(Guid? OrganizationId = null, int Page = 1, int PageSize = 50,
    string? SortBy = null, string? SortDir = "asc") : IRequest<PagedResult<RoleDto>>;

public class ListRolesQueryHandler : IRequestHandler<ListRolesQuery, PagedResult<RoleDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListRolesQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<RoleDto>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        // System roles (OrganizationId == null) are visible to every org, plus that org's own custom roles.
        var query = _db.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Where(r => r.OrganizationId == null || r.OrganizationId == request.OrganizationId);

        var total = await query.CountAsync(cancellationToken);
        var sorted = query.ApplySort(request.SortBy, request.SortDir, nameof(Role.Name));

        var items = await sorted.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);

        return new PagedResult<RoleDto>(items, total, request.Page, request.PageSize);
    }
}

public record ListPermissionsQuery : IRequest<IReadOnlyList<PermissionDto>>;

public class ListPermissionsQueryHandler : IRequestHandler<ListPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListPermissionsQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<IReadOnlyList<PermissionDto>> Handle(ListPermissionsQuery request, CancellationToken cancellationToken) =>
        await _db.Permissions.OrderBy(p => p.Category).ThenBy(p => p.Key)
            .ProjectTo<PermissionDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
}
