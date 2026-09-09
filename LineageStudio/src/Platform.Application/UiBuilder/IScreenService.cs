namespace Platform.Application.UiBuilder;

public interface IScreenService
{
    Task<IReadOnlyList<ScreenDto>> ListAsync(Guid applicationId, CancellationToken ct = default);
    Task<ScreenDto> GetAsync(Guid applicationId, Guid screenId, CancellationToken ct = default);
    Task<ScreenDto> CreateAsync(Guid applicationId, CreateScreenRequest request, CancellationToken ct = default);
    Task<ScreenDto> UpdateAsync(Guid applicationId, Guid screenId, UpdateScreenRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid applicationId, Guid screenId, CancellationToken ct = default);
}
