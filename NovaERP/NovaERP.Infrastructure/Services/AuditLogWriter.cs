using NovaERP.Application.Interfaces;
using NovaERP.Domain.Entities;
using NovaERP.Infrastructure.Data;

namespace NovaERP.Infrastructure.Services;

public class AuditLogWriter : IAuditLogWriter
{
    private readonly NovaErpDbContext _ctx;

    public AuditLogWriter(NovaErpDbContext ctx) => _ctx = ctx;

    public async Task WriteAsync(int? organizationId, int? userId, string? userName, string method,
        string path, string action, int statusCode, string? ipAddress, long durationMs)
    {
        _ctx.AuditLogs.Add(new AuditLog
        {
            OrganizationId = organizationId,
            UserId         = userId,
            UserName       = userName,
            Method         = method,
            Path           = path,
            Action         = action,
            StatusCode     = statusCode,
            IpAddress      = ipAddress,
            DurationMs     = durationMs
        });
        await _ctx.SaveChangesAsync();
    }
}
