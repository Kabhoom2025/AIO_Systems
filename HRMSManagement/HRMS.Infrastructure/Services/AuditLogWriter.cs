using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Services;

public class AuditLogWriter : IAuditLogWriter
{
    private readonly HrmsDbContext _ctx;

    public AuditLogWriter(HrmsDbContext ctx) => _ctx = ctx;

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
