using LinkShield.Application.DTOs.Scans;

namespace LinkShield.Application.Interfaces;

public interface IScanService
{
    Task<ScanDetailDto> SubmitScanAsync(string rawUrl, Guid? userId, Guid? apiClientId = null, CancellationToken ct = default);
    Task<ScanDetailDto?> GetScanAsync(Guid scanId, CancellationToken ct = default);
    Task<PagedResultDto<ScanSummaryDto>> GetScansAsync(int page, int pageSize, Guid? userId, CancellationToken ct = default);
    Task<bool> DeleteScanAsync(Guid scanId, CancellationToken ct = default);
}
