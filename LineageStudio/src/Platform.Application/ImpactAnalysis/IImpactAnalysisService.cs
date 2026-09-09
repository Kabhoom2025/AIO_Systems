namespace Platform.Application.ImpactAnalysis;

public interface IImpactAnalysisService
{
    Task<TableImpactDto> GetTableImpactAsync(Guid tableId, CancellationToken ct = default);
    Task<ColumnImpactDto> GetColumnImpactAsync(Guid columnId, CancellationToken ct = default);
}
