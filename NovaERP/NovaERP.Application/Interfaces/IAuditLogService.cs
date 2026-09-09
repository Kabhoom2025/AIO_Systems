using NovaERP.Application.Common;
using NovaERP.Application.DTOs;

namespace NovaERP.Application.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(int orgId, int page, int pageSize, string? search);
}
