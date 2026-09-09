using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Platform.Application.UiBuilder;
using Platform.Infrastructure.Persistence;
using ScreenEntity = Platform.Domain.Entities.Screen;

namespace Platform.Infrastructure.UiBuilder;

public class ScreenService : IScreenService
{
    private static readonly Regex ValidRoute = new(@"^/[a-zA-Z0-9\-_/:]*$", RegexOptions.Compiled);

    private readonly PlatformDbContext _db;

    public ScreenService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ScreenDto>> ListAsync(Guid applicationId, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        return await _db.Screens
            .Where(s => s.ApplicationId == applicationId)
            .OrderBy(s => s.Name)
            .Select(s => new ScreenDto(s.Id, s.ApplicationId, s.Name, s.Route, s.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ScreenDto> GetAsync(Guid applicationId, Guid screenId, CancellationToken ct = default)
    {
        var screen = await LoadAsync(applicationId, screenId, ct);
        return ToDto(screen);
    }

    public async Task<ScreenDto> CreateAsync(Guid applicationId, CreateScreenRequest request, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var name = request.Name.Trim();
        var route = NormalizeRoute(request.Route);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Screen name is required.");

        if (await _db.Screens.AnyAsync(s => s.ApplicationId == applicationId && s.Route == route, ct))
            throw new InvalidOperationException($"A screen with route '{route}' already exists in this application.");

        var screen = new ScreenEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = name,
            Route = route,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Screens.Add(screen);
        await _db.SaveChangesAsync(ct);
        return ToDto(screen);
    }

    public async Task<ScreenDto> UpdateAsync(Guid applicationId, Guid screenId, UpdateScreenRequest request, CancellationToken ct = default)
    {
        var screen = await LoadAsync(applicationId, screenId, ct);

        var name = request.Name.Trim();
        var route = NormalizeRoute(request.Route);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Screen name is required.");

        if (!string.Equals(screen.Route, route, StringComparison.Ordinal) &&
            await _db.Screens.AnyAsync(s => s.ApplicationId == applicationId && s.Route == route && s.Id != screenId, ct))
            throw new InvalidOperationException($"A screen with route '{route}' already exists in this application.");

        screen.Name = name;
        screen.Route = route;

        await _db.SaveChangesAsync(ct);
        return ToDto(screen);
    }

    public async Task DeleteAsync(Guid applicationId, Guid screenId, CancellationToken ct = default)
    {
        var screen = await LoadAsync(applicationId, screenId, ct);
        _db.Screens.Remove(screen);
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureApplicationExistsAsync(Guid applicationId, CancellationToken ct)
    {
        if (!await _db.Applications.AnyAsync(a => a.Id == applicationId, ct))
            throw new KeyNotFoundException($"Application '{applicationId}' was not found.");
    }

    private async Task<ScreenEntity> LoadAsync(Guid applicationId, Guid screenId, CancellationToken ct)
    {
        return await _db.Screens.FirstOrDefaultAsync(s => s.ApplicationId == applicationId && s.Id == screenId, ct)
            ?? throw new KeyNotFoundException($"Screen '{screenId}' was not found.");
    }

    private static string NormalizeRoute(string route)
    {
        var trimmed = route.Trim();
        if (!trimmed.StartsWith('/'))
            trimmed = "/" + trimmed;

        if (!ValidRoute.IsMatch(trimmed))
            throw new ArgumentException($"Route '{route}' is invalid. Use a path like '/customer-registration'.");

        return trimmed;
    }

    private static ScreenDto ToDto(ScreenEntity screen) =>
        new(screen.Id, screen.ApplicationId, screen.Name, screen.Route, screen.CreatedAt);
}
