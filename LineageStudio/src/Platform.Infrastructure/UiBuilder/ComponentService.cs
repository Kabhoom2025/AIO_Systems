using Microsoft.EntityFrameworkCore;
using Platform.Application.UiBuilder;
using Platform.Infrastructure.Common;
using Platform.Infrastructure.Persistence;
using ComponentEntity = Platform.Domain.Entities.UiComponent;

namespace Platform.Infrastructure.UiBuilder;

public class ComponentService : IComponentService
{
    private readonly PlatformDbContext _db;

    public ComponentService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ComponentDto>> ListAsync(Guid applicationId, Guid? screenId, CancellationToken ct = default)
    {
        var query = _db.Components.Where(c => c.Screen!.ApplicationId == applicationId);
        if (screenId is not null)
            query = query.Where(c => c.ScreenId == screenId);

        var components = await query.OrderBy(c => c.Name).ToListAsync(ct);
        return components.Select(ToDto).ToList();
    }

    public async Task<ComponentDto> GetAsync(Guid applicationId, Guid componentId, CancellationToken ct = default)
    {
        var component = await LoadAsync(applicationId, componentId, ct);
        return ToDto(component);
    }

    public async Task<ComponentDto> CreateAsync(Guid applicationId, CreateComponentRequest request, CancellationToken ct = default)
    {
        var screen = await _db.Screens.FirstOrDefaultAsync(s => s.ApplicationId == applicationId && s.Id == request.ScreenId, ct)
            ?? throw new KeyNotFoundException($"Screen '{request.ScreenId}' was not found in this application.");

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Component name is required.");

        var component = new ComponentEntity
        {
            Id = Guid.NewGuid(),
            ScreenId = screen.Id,
            Type = request.Type,
            Name = name,
            PositionX = request.PositionX,
            PositionY = request.PositionY,
            PropertiesJson = JsonValidation.ValidateOrNull(request.PropertiesJson, "Properties"),
            ValidationJson = JsonValidation.ValidateOrNull(request.ValidationJson, "Validation"),
            DataBinding = string.IsNullOrWhiteSpace(request.DataBinding) ? null : request.DataBinding.Trim(),
            EventsJson = JsonValidation.ValidateOrNull(request.EventsJson, "Events"),
        };

        _db.Components.Add(component);
        await _db.SaveChangesAsync(ct);
        return ToDto(component);
    }

    public async Task<ComponentDto> UpdateAsync(Guid applicationId, Guid componentId, UpdateComponentRequest request, CancellationToken ct = default)
    {
        var component = await LoadAsync(applicationId, componentId, ct);

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Component name is required.");

        component.Name = name;
        component.PositionX = request.PositionX;
        component.PositionY = request.PositionY;
        component.PropertiesJson = JsonValidation.ValidateOrNull(request.PropertiesJson, "Properties");
        component.ValidationJson = JsonValidation.ValidateOrNull(request.ValidationJson, "Validation");
        component.DataBinding = string.IsNullOrWhiteSpace(request.DataBinding) ? null : request.DataBinding.Trim();
        component.EventsJson = JsonValidation.ValidateOrNull(request.EventsJson, "Events");

        await _db.SaveChangesAsync(ct);
        return ToDto(component);
    }

    public async Task DeleteAsync(Guid applicationId, Guid componentId, CancellationToken ct = default)
    {
        var component = await LoadAsync(applicationId, componentId, ct);
        _db.Components.Remove(component);
        await _db.SaveChangesAsync(ct);
    }

    private async Task<ComponentEntity> LoadAsync(Guid applicationId, Guid componentId, CancellationToken ct)
    {
        return await _db.Components
            .FirstOrDefaultAsync(c => c.Screen!.ApplicationId == applicationId && c.Id == componentId, ct)
            ?? throw new KeyNotFoundException($"Component '{componentId}' was not found.");
    }

    private static ComponentDto ToDto(ComponentEntity c) => new(
        c.Id, c.ScreenId, c.Type, c.Name, c.PositionX, c.PositionY,
        c.PropertiesJson, c.ValidationJson, c.DataBinding, c.EventsJson);
}
