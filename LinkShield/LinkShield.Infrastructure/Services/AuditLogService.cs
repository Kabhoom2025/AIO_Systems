using LinkShield.Application.DTOs.Admin;
using LinkShield.Application.DTOs.Scans;
using LinkShield.Application.Interfaces;
using LinkShield.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkShield.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly LinkShieldDbContext _db;

    public AuditLogService(LinkShieldDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<AuditLogDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.AuditLogs.Include(a => a.User).OrderByDescending(a => a.OccurredAtUtc);
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(a.Id, a.UserId, a.User == null ? null : a.User.Email, a.Action, a.EntityType, a.EntityId, a.DetailsJson, a.IpAddress, a.OccurredAtUtc))
            .ToListAsync(ct);

        return new PagedResultDto<AuditLogDto>(items, page, pageSize, totalCount);
    }
}
