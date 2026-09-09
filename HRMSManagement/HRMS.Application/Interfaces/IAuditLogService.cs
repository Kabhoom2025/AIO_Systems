using HRMS.Application.Common;
using HRMS.Application.DTOs;

namespace HRMS.Application.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(int orgId, int page, int pageSize, string? search);
}
