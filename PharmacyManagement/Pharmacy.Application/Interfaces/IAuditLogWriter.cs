namespace Pharmacy.Application.Interfaces;

public interface IAuditLogWriter
{
    Task WriteAsync(int organizationId, int userId, string userName, string httpMethod, string action, string entityType, int? entityId);
}
