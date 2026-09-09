namespace Platform.Application.Applications;

public interface IApplicationService
{
    Task<IReadOnlyList<ApplicationDto>> ListAsync(CancellationToken ct = default);
    Task<ApplicationDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<ApplicationDto> CreateAsync(CreateApplicationRequest request, CancellationToken ct = default);
    Task<ApplicationDto> UpdateAsync(Guid id, UpdateApplicationRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Snapshots the application's current design-time configuration into a new,
    /// immutable version and marks the application published against it.</summary>
    Task<ApplicationVersionDto> PublishAsync(Guid id, CancellationToken ct = default);

    Task<ApplicationDto> UnpublishAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ApplicationVersionDto>> ListVersionsAsync(Guid id, CancellationToken ct = default);
}
