namespace Platform.Application.ApiDesigner;

public interface IApiEndpointService
{
    Task<IReadOnlyList<ApiEndpointDto>> ListAsync(Guid applicationId, CancellationToken ct = default);
    Task<ApiEndpointDto> GetAsync(Guid applicationId, Guid apiId, CancellationToken ct = default);
    Task<ApiEndpointDto> CreateAsync(Guid applicationId, CreateApiEndpointRequest request, CancellationToken ct = default);
    Task<ApiEndpointDto> UpdateAsync(Guid applicationId, Guid apiId, UpdateApiEndpointRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid applicationId, Guid apiId, CancellationToken ct = default);
}
