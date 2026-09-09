namespace Platform.Application.ApiDesigner;

public interface IServiceDefinitionService
{
    Task<IReadOnlyList<ServiceDto>> ListAsync(Guid applicationId, CancellationToken ct = default);
    Task<ServiceDto> GetAsync(Guid applicationId, Guid serviceId, CancellationToken ct = default);
    Task<ServiceDto> CreateAsync(Guid applicationId, CreateServiceRequest request, CancellationToken ct = default);
    Task<ServiceDto> UpdateAsync(Guid applicationId, Guid serviceId, UpdateServiceRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid applicationId, Guid serviceId, CancellationToken ct = default);
}
