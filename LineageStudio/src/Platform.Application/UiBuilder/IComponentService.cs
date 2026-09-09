namespace Platform.Application.UiBuilder;

public interface IComponentService
{
    /// <summary>Lists components for an application, optionally narrowed to one screen.</summary>
    Task<IReadOnlyList<ComponentDto>> ListAsync(Guid applicationId, Guid? screenId, CancellationToken ct = default);
    Task<ComponentDto> GetAsync(Guid applicationId, Guid componentId, CancellationToken ct = default);
    Task<ComponentDto> CreateAsync(Guid applicationId, CreateComponentRequest request, CancellationToken ct = default);
    Task<ComponentDto> UpdateAsync(Guid applicationId, Guid componentId, UpdateComponentRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid applicationId, Guid componentId, CancellationToken ct = default);
}
