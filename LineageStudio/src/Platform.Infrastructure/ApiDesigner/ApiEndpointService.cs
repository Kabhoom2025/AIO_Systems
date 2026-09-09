using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Platform.Application.ApiDesigner;
using Platform.Infrastructure.Common;
using Platform.Infrastructure.Persistence;
using ApiEndpointEntity = Platform.Domain.Entities.ApiEndpoint;

namespace Platform.Infrastructure.ApiDesigner;

public class ApiEndpointService : IApiEndpointService
{
    private static readonly Regex ValidPath = new(@"^/[a-zA-Z0-9\-_/{}:]*$", RegexOptions.Compiled);

    private readonly PlatformDbContext _db;

    public ApiEndpointService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ApiEndpointDto>> ListAsync(Guid applicationId, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var apis = await _db.Apis
            .Where(a => a.ApplicationId == applicationId)
            .OrderBy(a => a.Path)
            .ToListAsync(ct);

        return apis.Select(ToDto).ToList();
    }

    public async Task<ApiEndpointDto> GetAsync(Guid applicationId, Guid apiId, CancellationToken ct = default)
    {
        var api = await LoadAsync(applicationId, apiId, ct);
        return ToDto(api);
    }

    public async Task<ApiEndpointDto> CreateAsync(Guid applicationId, CreateApiEndpointRequest request, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var path = NormalizePath(request.Path);
        await ValidateReferencesAsync(applicationId, request.ServiceId, request.TableId, ct);

        if (await _db.Apis.AnyAsync(a => a.ApplicationId == applicationId && a.Method == request.Method && a.Path == path, ct))
            throw new InvalidOperationException($"{request.Method} {path} already exists in this application.");

        var api = new ApiEndpointEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Method = request.Method,
            Path = path,
            RequestSchemaJson = JsonValidation.ValidateOrNull(request.RequestSchemaJson, "Request schema"),
            ResponseSchemaJson = JsonValidation.ValidateOrNull(request.ResponseSchemaJson, "Response schema"),
            ServiceId = request.ServiceId,
            TableId = request.TableId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Apis.Add(api);
        await _db.SaveChangesAsync(ct);
        return ToDto(api);
    }

    public async Task<ApiEndpointDto> UpdateAsync(Guid applicationId, Guid apiId, UpdateApiEndpointRequest request, CancellationToken ct = default)
    {
        var api = await LoadAsync(applicationId, apiId, ct);

        var path = NormalizePath(request.Path);
        await ValidateReferencesAsync(applicationId, request.ServiceId, request.TableId, ct);

        var changingRoute = api.Method != request.Method || !string.Equals(api.Path, path, StringComparison.Ordinal);
        if (changingRoute &&
            await _db.Apis.AnyAsync(a => a.ApplicationId == applicationId && a.Method == request.Method && a.Path == path && a.Id != apiId, ct))
            throw new InvalidOperationException($"{request.Method} {path} already exists in this application.");

        api.Method = request.Method;
        api.Path = path;
        api.RequestSchemaJson = JsonValidation.ValidateOrNull(request.RequestSchemaJson, "Request schema");
        api.ResponseSchemaJson = JsonValidation.ValidateOrNull(request.ResponseSchemaJson, "Response schema");
        api.ServiceId = request.ServiceId;
        api.TableId = request.TableId;

        await _db.SaveChangesAsync(ct);
        return ToDto(api);
    }

    public async Task DeleteAsync(Guid applicationId, Guid apiId, CancellationToken ct = default)
    {
        var api = await LoadAsync(applicationId, apiId, ct);
        _db.Apis.Remove(api);
        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateReferencesAsync(Guid applicationId, Guid? serviceId, Guid? tableId, CancellationToken ct)
    {
        if (serviceId is Guid sid && !await _db.Services.AnyAsync(s => s.ApplicationId == applicationId && s.Id == sid, ct))
            throw new ArgumentException($"Service '{sid}' was not found in this application.");

        if (tableId is Guid tid && !await _db.Tables.AnyAsync(t => t.ApplicationId == applicationId && t.Id == tid, ct))
            throw new ArgumentException($"Table '{tid}' was not found in this application.");
    }

    private async Task EnsureApplicationExistsAsync(Guid applicationId, CancellationToken ct)
    {
        if (!await _db.Applications.AnyAsync(a => a.Id == applicationId, ct))
            throw new KeyNotFoundException($"Application '{applicationId}' was not found.");
    }

    private async Task<ApiEndpointEntity> LoadAsync(Guid applicationId, Guid apiId, CancellationToken ct)
    {
        return await _db.Apis.FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.Id == apiId, ct)
            ?? throw new KeyNotFoundException($"API '{apiId}' was not found.");
    }

    private static string NormalizePath(string path)
    {
        var trimmed = path.Trim();
        if (!trimmed.StartsWith('/'))
            trimmed = "/" + trimmed;

        if (!ValidPath.IsMatch(trimmed))
            throw new ArgumentException($"Path '{path}' is invalid. Use a path like '/customer' or '/customer/{{id}}'.");

        return trimmed;
    }

    private static ApiEndpointDto ToDto(ApiEndpointEntity a) => new(
        a.Id, a.ApplicationId, a.Method, a.Path, a.RequestSchemaJson, a.ResponseSchemaJson, a.ServiceId, a.TableId, a.CreatedAt);
}
