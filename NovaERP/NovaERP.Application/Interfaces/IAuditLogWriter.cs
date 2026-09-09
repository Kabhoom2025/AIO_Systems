namespace NovaERP.Application.Interfaces;

public interface IAuditLogWriter
{
    Task WriteAsync(int? organizationId, int? userId, string? userName, string method,
        string path, string action, int statusCode, string? ipAddress, long durationMs);
}
