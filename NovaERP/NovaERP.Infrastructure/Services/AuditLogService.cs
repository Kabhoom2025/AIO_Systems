using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common;
using NovaERP.Application.DTOs;
using NovaERP.Application.Interfaces;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly NovaErpDbContext _ctx;
    private readonly IMapper _mapper;

    public AuditLogService(NovaErpDbContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(int orgId, int page, int pageSize, string? search)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var query = _ctx.AuditLogs.Where(a => a.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(a =>
                (a.UserName ?? "").ToLower().Contains(s) ||
                a.Path.ToLower().Contains(s) ||
                a.Action.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items      = items.Select(_mapper.Map<AuditLogDto>).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize
        };
    }
}
