using Microsoft.EntityFrameworkCore;
using Platform.Application.ImpactAnalysis;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.ImpactAnalysis;

public class ImpactAnalysisService : IImpactAnalysisService
{
    private readonly PlatformDbContext _db;

    public ImpactAnalysisService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<TableImpactDto> GetTableImpactAsync(Guid tableId, CancellationToken ct = default)
    {
        var table = await _db.Tables.Include(t => t.Columns).FirstOrDefaultAsync(t => t.Id == tableId, ct)
            ?? throw new KeyNotFoundException($"Table '{tableId}' was not found.");

        var columnIds = table.Columns.Select(c => c.Id).ToList();

        var mappings = await _db.Mappings
            .Where(m => columnIds.Contains(m.ColumnId))
            .ToListAsync(ct);

        var componentIds = mappings.Select(m => m.SourceComponentId).Distinct().ToList();
        var components = await _db.Components
            .Include(c => c.Screen)
            .Where(c => componentIds.Contains(c.Id))
            .ToListAsync(ct);

        var screens = components
            .Select(c => c.Screen)
            .Where(s => s is not null)
            .DistinctBy(s => s!.Id)
            .Select(s => new ScreenRefDto(s!.Id, s.Name, s.Route))
            .ToList();

        var directApiIds = await _db.Apis.Where(a => a.TableId == tableId).Select(a => a.Id).ToListAsync(ct);
        var mappedApiIds = mappings.Select(m => m.ApiId);
        var apiIds = directApiIds.Concat(mappedApiIds).Distinct().ToList();
        var apis = await _db.Apis
            .Where(a => apiIds.Contains(a.Id))
            .Select(a => new ApiRefDto(a.Id, a.Method.ToString(), a.Path))
            .ToListAsync(ct);

        var directServiceIds = await _db.Services.Where(s => s.TableId == tableId).Select(s => s.Id).ToListAsync(ct);
        var mappedServiceIds = mappings.Where(m => m.ServiceId.HasValue).Select(m => m.ServiceId!.Value);
        var serviceIds = directServiceIds.Concat(mappedServiceIds).Distinct().ToList();
        var services = await _db.Services
            .Where(s => serviceIds.Contains(s.Id))
            .Select(s => new ServiceRefDto(s.Id, s.Name))
            .ToListAsync(ct);

        var referencingTables = await _db.Tables
            .Where(t => t.Id != tableId && t.Columns.Any(c => c.ReferencesTableId == tableId))
            .Select(t => new TableRefDto(t.Id, t.Name))
            .ToListAsync(ct);

        var referencedTableIds = table.Columns
            .Where(c => c.ReferencesTableId.HasValue)
            .Select(c => c.ReferencesTableId!.Value)
            .Distinct()
            .ToList();
        var referencedTables = await _db.Tables
            .Where(t => referencedTableIds.Contains(t.Id))
            .Select(t => new TableRefDto(t.Id, t.Name))
            .ToListAsync(ct);

        return new TableImpactDto(table.Id, table.Name, screens, apis, services, referencingTables, referencedTables);
    }

    public async Task<ColumnImpactDto> GetColumnImpactAsync(Guid columnId, CancellationToken ct = default)
    {
        var column = await _db.Columns.Include(c => c.Table).FirstOrDefaultAsync(c => c.Id == columnId, ct)
            ?? throw new KeyNotFoundException($"Column '{columnId}' was not found.");

        var mappings = await _db.Mappings.Where(m => m.ColumnId == columnId).ToListAsync(ct);

        var componentIds = mappings.Select(m => m.SourceComponentId).Distinct().ToList();
        var components = await _db.Components
            .Include(c => c.Screen)
            .Where(c => componentIds.Contains(c.Id))
            .ToListAsync(ct);
        var componentRefs = components
            .Where(c => c.Screen is not null)
            .Select(c => new ComponentRefDto(c.Id, c.Name, c.Screen!.Id, c.Screen.Name))
            .ToList();

        var apiIds = mappings.Select(m => m.ApiId).Distinct().ToList();
        var apisById = (await _db.Apis.Where(a => apiIds.Contains(a.Id)).ToListAsync(ct)).ToDictionary(a => a.Id);
        var apiFields = mappings
            .Where(m => apisById.ContainsKey(m.ApiId))
            .Select(m => new ApiFieldRefDto(m.ApiId, apisById[m.ApiId].Method.ToString(), apisById[m.ApiId].Path, m.ApiField))
            .DistinctBy(f => (f.ApiId, f.ApiField))
            .ToList();

        var serviceMappings = mappings.Where(m => m.ServiceId.HasValue && m.ServiceField is not null).ToList();
        var serviceIds = serviceMappings.Select(m => m.ServiceId!.Value).Distinct().ToList();
        var servicesById = (await _db.Services.Where(s => serviceIds.Contains(s.Id)).ToListAsync(ct)).ToDictionary(s => s.Id);
        var serviceFields = serviceMappings
            .Where(m => servicesById.ContainsKey(m.ServiceId!.Value))
            .Select(m => new ServiceFieldRefDto(m.ServiceId!.Value, servicesById[m.ServiceId.Value].Name, m.ServiceField!))
            .DistinctBy(f => (f.ServiceId, f.ServiceField))
            .ToList();

        var dependentColumns = await _db.Columns
            .Include(c => c.Table)
            .Where(c => c.ReferencesColumnId == columnId)
            .Select(c => new ColumnRefDto(c.Id, c.Name, c.TableId, c.Table!.Name))
            .ToListAsync(ct);

        return new ColumnImpactDto(
            column.Id, column.Name, column.TableId, column.Table!.Name,
            componentRefs, apiFields, serviceFields, dependentColumns);
    }
}
