using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.Infrastructure.Services;

public class AuditLogWriter : IAuditLogWriter
{
    private readonly PharmacyDbContext _ctx;

    public AuditLogWriter(PharmacyDbContext ctx) => _ctx = ctx;

    public async Task WriteAsync(int organizationId, int userId, string userName, string httpMethod, string action, string entityType, int? entityId)
    {
        _ctx.AuditLogs.Add(new AuditLog
        {
            OrganizationId = organizationId,
            UserId         = userId,
            UserName       = userName,
            HttpMethod     = httpMethod,
            Action         = action,
            EntityType     = entityType,
            EntityId       = entityId
        });
        await _ctx.SaveChangesAsync();
    }
}
