using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly HrmsDbContext _ctx;

    public AuditLogService(HrmsDbContext ctx) => _ctx = ctx;

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
            .Select(a => new AuditLogDto
            {
                Id          = a.Id,
                UserId      = a.UserId,
                UserName    = a.UserName,
                Action      = a.Action,
                Method      = a.Method,
                Path        = a.Path,
                StatusCode  = a.StatusCode,
                IpAddress   = a.IpAddress,
                DurationMs  = a.DurationMs,
                CreatedDate = a.CreatedDate
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
