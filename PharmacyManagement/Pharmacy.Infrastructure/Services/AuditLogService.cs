using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.DTOs;
using Pharmacy.Application.Interfaces;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly PharmacyDbContext _ctx;

    public AuditLogService(PharmacyDbContext ctx) => _ctx = ctx;

    public Task<List<AuditLogDto>> GetRecentAsync(int orgId, int take = 200) =>
        _ctx.AuditLogs
            .Where(a => a.OrganizationId == orgId)
            .OrderByDescending(a => a.Timestamp)
            .Take(take)
            .Select(a => new AuditLogDto
            {
                Id         = a.Id,
                Timestamp  = a.Timestamp,
                UserName   = a.UserName,
                Action     = a.Action,
                EntityType = a.EntityType,
                EntityId   = a.EntityId
            })
            .ToListAsync();
}
