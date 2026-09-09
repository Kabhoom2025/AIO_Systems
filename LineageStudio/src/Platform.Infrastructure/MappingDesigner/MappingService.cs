using Microsoft.EntityFrameworkCore;
using Platform.Application.MappingDesigner;
using Platform.Domain.Enums;
using Platform.Infrastructure.Common;
using Platform.Infrastructure.Persistence;
using MappingEntity = Platform.Domain.Entities.FieldMapping;

namespace Platform.Infrastructure.MappingDesigner;

public class MappingService : IMappingService
{
    private static readonly HashSet<TransformationType> TransformationsRequiringConfig =
    [
        TransformationType.Default,
        TransformationType.Concatenate,
        TransformationType.Split,
        TransformationType.DateConversion,
        TransformationType.NumberConversion,
    ];

    private readonly PlatformDbContext _db;

    public MappingService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MappingDto>> ListAsync(Guid applicationId, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var mappings = await _db.Mappings
            .Where(m => m.ApplicationId == applicationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        return mappings.Select(ToDto).ToList();
    }

    public async Task<MappingDto> GetAsync(Guid applicationId, Guid mappingId, CancellationToken ct = default)
    {
        var mapping = await LoadAsync(applicationId, mappingId, ct);
        return ToDto(mapping);
    }

    public async Task<MappingDto> CreateAsync(Guid applicationId, CreateMappingRequest request, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);
        await ValidateReferencesAsync(applicationId, request.SourceComponentId, request.ApiId, request.ServiceId, request.ColumnId, ct);

        var sourceField = RequireField(request.SourceField, "Source field");
        var apiField = RequireField(request.ApiField, "API field");
        var serviceField = string.IsNullOrWhiteSpace(request.ServiceField) ? null : request.ServiceField.Trim();
        var configJson = ValidateTransformation(request.Transformation, request.TransformationConfigJson);

        var mapping = new MappingEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            SourceComponentId = request.SourceComponentId,
            SourceField = sourceField,
            ApiId = request.ApiId,
            ApiField = apiField,
            ServiceId = request.ServiceId,
            ServiceField = serviceField,
            ColumnId = request.ColumnId,
            Transformation = request.Transformation,
            TransformationConfigJson = configJson,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Mappings.Add(mapping);
        await _db.SaveChangesAsync(ct);
        return ToDto(mapping);
    }

    public async Task<MappingDto> UpdateAsync(Guid applicationId, Guid mappingId, UpdateMappingRequest request, CancellationToken ct = default)
    {
        var mapping = await LoadAsync(applicationId, mappingId, ct);

        mapping.SourceField = RequireField(request.SourceField, "Source field");
        mapping.ApiField = RequireField(request.ApiField, "API field");
        mapping.ServiceField = string.IsNullOrWhiteSpace(request.ServiceField) ? null : request.ServiceField.Trim();
        mapping.Transformation = request.Transformation;
        mapping.TransformationConfigJson = ValidateTransformation(request.Transformation, request.TransformationConfigJson);

        await _db.SaveChangesAsync(ct);
        return ToDto(mapping);
    }

    public async Task DeleteAsync(Guid applicationId, Guid mappingId, CancellationToken ct = default)
    {
        var mapping = await LoadAsync(applicationId, mappingId, ct);
        _db.Mappings.Remove(mapping);
        await _db.SaveChangesAsync(ct);
    }

    private async Task ValidateReferencesAsync(
        Guid applicationId, Guid sourceComponentId, Guid apiId, Guid? serviceId, Guid columnId, CancellationToken ct)
    {
        if (!await _db.Components.AnyAsync(c => c.Screen!.ApplicationId == applicationId && c.Id == sourceComponentId, ct))
            throw new ArgumentException($"Component '{sourceComponentId}' was not found in this application.");

        if (!await _db.Apis.AnyAsync(a => a.ApplicationId == applicationId && a.Id == apiId, ct))
            throw new ArgumentException($"API '{apiId}' was not found in this application.");

        if (serviceId is Guid sid && !await _db.Services.AnyAsync(s => s.ApplicationId == applicationId && s.Id == sid, ct))
            throw new ArgumentException($"Service '{sid}' was not found in this application.");

        if (!await _db.Columns.AnyAsync(c => c.Table!.ApplicationId == applicationId && c.Id == columnId, ct))
            throw new ArgumentException($"Column '{columnId}' was not found in this application.");
    }

    private static string RequireField(string value, string fieldName)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException($"{fieldName} is required.");
        return trimmed;
    }

    /// <summary>Transformation is restricted to this fixed enum of server-known operations - never
    /// arbitrary code - so all this needs to check is that config is present where the
    /// transformation needs one, and that it's well-formed JSON.</summary>
    private static string? ValidateTransformation(TransformationType transformation, string? configJson)
    {
        var validated = JsonValidation.ValidateOrNull(configJson, "Transformation config");

        if (TransformationsRequiringConfig.Contains(transformation) && validated is null)
            throw new ArgumentException($"Transformation '{transformation}' requires a transformationConfigJson.");

        return validated;
    }

    private async Task EnsureApplicationExistsAsync(Guid applicationId, CancellationToken ct)
    {
        if (!await _db.Applications.AnyAsync(a => a.Id == applicationId, ct))
            throw new KeyNotFoundException($"Application '{applicationId}' was not found.");
    }

    private async Task<MappingEntity> LoadAsync(Guid applicationId, Guid mappingId, CancellationToken ct)
    {
        return await _db.Mappings.FirstOrDefaultAsync(m => m.ApplicationId == applicationId && m.Id == mappingId, ct)
            ?? throw new KeyNotFoundException($"Mapping '{mappingId}' was not found.");
    }

    private static MappingDto ToDto(MappingEntity m) => new(
        m.Id, m.ApplicationId, m.SourceComponentId, m.SourceField, m.ApiId, m.ApiField,
        m.ServiceId, m.ServiceField, m.ColumnId, m.Transformation, m.TransformationConfigJson, m.CreatedAt);
}
