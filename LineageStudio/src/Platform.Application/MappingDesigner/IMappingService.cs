namespace Platform.Application.MappingDesigner;

public interface IMappingService
{
    Task<IReadOnlyList<MappingDto>> ListAsync(Guid applicationId, CancellationToken ct = default);
    Task<MappingDto> GetAsync(Guid applicationId, Guid mappingId, CancellationToken ct = default);
    Task<MappingDto> CreateAsync(Guid applicationId, CreateMappingRequest request, CancellationToken ct = default);
    Task<MappingDto> UpdateAsync(Guid applicationId, Guid mappingId, UpdateMappingRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid applicationId, Guid mappingId, CancellationToken ct = default);
}
