using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectFlowAI.Application.Common;
using ProjectFlowAI.Application.DTOs;
using ProjectFlowAI.Application.Interfaces;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Application.Features.Users;

public record ListUsersQuery(Guid? OrganizationId = null, int Page = 1, int PageSize = 20,
    string? SortBy = null, string? SortDir = "asc", string? Search = null) : IRequest<PagedResult<UserDto>>;

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, PagedResult<UserDto>>
{
    private readonly IProjectFlowDbContext _db;
    private readonly IMapper _mapper;

    public ListUsersQueryHandler(IProjectFlowDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<PagedResult<UserDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Users.AsQueryable();
        if (request.OrganizationId.HasValue)
            query = query.Where(u => u.OrganizationId == request.OrganizationId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u => u.Email.Contains(request.Search) || u.FirstName.Contains(request.Search) || u.LastName.Contains(request.Search));

        var total = await query.CountAsync(cancellationToken);
        query = query.ApplySort(request.SortBy, request.SortDir, nameof(User.Email));

        var users = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Roles/Permissions aren't 1:1 properties on User (they're a join through UserRoles ->
        // Roles -> RolePermissions), so ProjectTo can't fill them — batch-fetch for this page's
        // user IDs instead of an N+1 per-row lookup.
        var userIds = users.Select(u => u.Id).ToList();
        var roleAssignments = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Select(ur => new { ur.UserId, ur.RoleId })
            .ToListAsync(cancellationToken);
        var roleIds = roleAssignments.Select(x => x.RoleId).Distinct().ToList();
        var roleNamesById = await _db.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);
        var rolePermissions = await _db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => new { rp.RoleId, PermissionKey = rp.Permission!.Key })
            .ToListAsync(cancellationToken);

        var items = users.Select(u =>
        {
            var userRoleIds = roleAssignments.Where(x => x.UserId == u.Id).Select(x => x.RoleId).ToList();
            var roles = userRoleIds.Select(rid => roleNamesById.GetValueOrDefault(rid)).Where(n => n != null).Select(n => n!).ToList();
            var permissions = rolePermissions.Where(rp => userRoleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionKey).Distinct().ToList();
            return new UserDto(u.Id, u.Email, u.FirstName, u.LastName, u.AvatarUrl, u.IsEmailVerified, u.TwoFactorEnabled,
                u.Status, u.LastLoginAt, u.OrganizationId, u.CreatedAt, roles, permissions);
        }).ToList();

        return new PagedResult<UserDto>(items, total, request.Page, request.PageSize);
    }
}
