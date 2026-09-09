using LinkShield.Application.DTOs.Admin;
using LinkShield.Application.DTOs.Scans;

namespace LinkShield.Application.Interfaces;

public interface IBrandManagementService
{
    Task<IReadOnlyList<BrandProfileDto>> GetAllAsync(CancellationToken ct = default);
    Task<BrandProfileDto> CreateAsync(CreateBrandProfileRequest request, Guid? actingUserId, CancellationToken ct = default);
}

public interface IRiskRuleManagementService
{
    Task<IReadOnlyList<RiskRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<RiskRuleDto> CreateAsync(CreateRiskRuleRequest request, Guid? actingUserId, CancellationToken ct = default);
    Task<RiskRuleDto> UpdateAsync(Guid id, UpdateRiskRuleRequest request, Guid? actingUserId, CancellationToken ct = default);
}

public interface IAuditLogService
{
    Task<PagedResultDto<AuditLogDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
}
