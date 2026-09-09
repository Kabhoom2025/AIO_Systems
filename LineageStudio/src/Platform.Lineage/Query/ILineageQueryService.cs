using Platform.Lineage.Recording;

namespace Platform.Lineage.Query;

public interface ILineageQueryService
{
    Task<IReadOnlyList<LineageExecutionDto>> ListExecutionsAsync(Guid? applicationId, CancellationToken ct = default);
    Task<LineageExecutionDto> GetExecutionAsync(Guid executionId, CancellationToken ct = default);
    Task<IReadOnlyList<LineageEventDto>> ListEventsAsync(Guid executionId, CancellationToken ct = default);
}
