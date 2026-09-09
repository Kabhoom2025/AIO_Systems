using Pharmacy.Application.DTOs;

namespace Pharmacy.Application.Interfaces;

public interface IAuditLogService
{
    Task<List<AuditLogDto>> GetRecentAsync(int orgId, int take = 200);
}
