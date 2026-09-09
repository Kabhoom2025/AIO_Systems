using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Platform.Application.Applications;
using Platform.Infrastructure.DataDesigner;
using Platform.Infrastructure.Persistence;
using AppEntity = Platform.Domain.Entities.AppDefinition;

namespace Platform.Infrastructure.Applications;

public class ApplicationService : IApplicationService
{
    private readonly PlatformDbContext _db;

    public ApplicationService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ApplicationDto>> ListAsync(CancellationToken ct = default)
    {
        return await _db.Applications
            .OrderBy(a => a.Name)
            .Select(a => new ApplicationDto(a.Id, a.Name, a.Description, a.IsPublished, a.CurrentVersionId, a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<ApplicationDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var app = await _db.Applications.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Application '{id}' was not found.");
        return ToDto(app);
    }

    public async Task<ApplicationDto> CreateAsync(CreateApplicationRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name is required.");

        if (await _db.Applications.AnyAsync(a => a.Name == name, ct))
            throw new InvalidOperationException($"An application named '{name}' already exists.");

        var now = DateTimeOffset.UtcNow;
        var app = new AppEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            IsPublished = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Applications.Add(app);
        await _db.SaveChangesAsync(ct);
        return ToDto(app);
    }

    public async Task<ApplicationDto> UpdateAsync(Guid id, UpdateApplicationRequest request, CancellationToken ct = default)
    {
        var app = await _db.Applications.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Application '{id}' was not found.");

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Application name is required.");

        if (!string.Equals(app.Name, name, StringComparison.Ordinal) &&
            await _db.Applications.AnyAsync(a => a.Name == name && a.Id != id, ct))
            throw new InvalidOperationException($"An application named '{name}' already exists.");

        app.Name = name;
        app.Description = request.Description?.Trim();
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(app);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var app = await _db.Applications.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Application '{id}' was not found.");

        // Deletes the metadata AND drops the application's entire real Postgres schema (every
        // table it ever created) in one transaction - otherwise every deleted app leaves its real
        // data permanently orphaned behind with no metadata left pointing at it.
        var schemaName = SqlIdentifiers.SchemaNameFor(id);
        var dropSchemaSql = DdlBuilder.DropSchema(schemaName);

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var connection = (NpgsqlConnection)_db.Database.GetDbConnection();
            var dbTransaction = (NpgsqlTransaction)transaction.GetDbTransaction();

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = dropSchemaSql;
                command.Transaction = dbTransaction;
                await command.ExecuteNonQueryAsync(ct);
            }

            _db.Applications.Remove(app);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ApplicationVersionDto> PublishAsync(Guid id, CancellationToken ct = default)
    {
        var app = await _db.Applications
            .Include(a => a.Screens).ThenInclude(s => s.Components)
            .Include(a => a.Tables).ThenInclude(t => t.Columns)
            .Include(a => a.Apis)
            .Include(a => a.Services)
            .Include(a => a.Mappings)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new KeyNotFoundException($"Application '{id}' was not found.");

        var nextVersionNumber = 1 + await _db.ApplicationVersions
            .Where(v => v.ApplicationId == id)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(ct) ?? 1;

        var screenSnapshots = new List<ScreenSnapshot>();
        foreach (var s in app.Screens)
        {
            var componentSnapshots = new List<ComponentSnapshot>();
            foreach (var c in s.Components)
            {
                componentSnapshots.Add(new ComponentSnapshot(
                    c.Id, c.Type.ToString(), c.Name, c.PositionX, c.PositionY,
                    c.PropertiesJson, c.ValidationJson, c.DataBinding, c.EventsJson));
            }
            screenSnapshots.Add(new ScreenSnapshot(s.Id, s.Name, s.Route, componentSnapshots));
        }

        var tableSnapshots = new List<TableSnapshot>();
        foreach (var t in app.Tables)
        {
            var columnSnapshots = new List<ColumnSnapshot>();
            foreach (var c in t.Columns)
            {
                columnSnapshots.Add(new ColumnSnapshot(
                    c.Id, c.Name, c.DataType.ToString(), c.Length, c.IsPrimaryKey, c.IsForeignKey,
                    c.ReferencesTableId, c.ReferencesColumnId, c.IsUnique, c.IsIndexed, c.IsNullable,
                    c.DefaultValue));
            }
            tableSnapshots.Add(new TableSnapshot(t.Id, t.Name, t.SchemaName, columnSnapshots));
        }

        var apiSnapshots = new List<ApiSnapshot>();
        foreach (var a in app.Apis)
        {
            apiSnapshots.Add(new ApiSnapshot(
                a.Id, a.Method.ToString(), a.Path, a.RequestSchemaJson, a.ResponseSchemaJson,
                a.ServiceId, a.TableId));
        }

        var serviceSnapshots = new List<ServiceSnapshot>();
        foreach (var s in app.Services)
        {
            serviceSnapshots.Add(new ServiceSnapshot(s.Id, s.Name, s.TableId, s.Description));
        }

        var mappingSnapshots = new List<MappingSnapshot>();
        foreach (var m in app.Mappings)
        {
            mappingSnapshots.Add(new MappingSnapshot(
                m.Id, m.SourceComponentId, m.SourceField, m.ApiId, m.ApiField, m.ServiceId,
                m.ServiceField, m.ColumnId, m.Transformation.ToString(), m.TransformationConfigJson));
        }

        var snapshot = new ApplicationSnapshot(
            app.Id,
            app.Name,
            app.Description,
            nextVersionNumber,
            screenSnapshots,
            tableSnapshots,
            apiSnapshots,
            serviceSnapshots,
            mappingSnapshots);

        var now = DateTimeOffset.UtcNow;
        var version = new Platform.Domain.Entities.ApplicationVersion
        {
            Id = Guid.NewGuid(),
            ApplicationId = app.Id,
            VersionNumber = nextVersionNumber,
            SnapshotJson = JsonSerializer.Serialize(snapshot),
            PublishedAt = now,
            CreatedAt = now,
        };

        _db.ApplicationVersions.Add(version);
        app.CurrentVersionId = version.Id;
        app.IsPublished = true;
        app.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        return new ApplicationVersionDto(version.Id, version.ApplicationId, version.VersionNumber, version.PublishedAt, version.CreatedAt);
    }

    public async Task<ApplicationDto> UnpublishAsync(Guid id, CancellationToken ct = default)
    {
        var app = await _db.Applications.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Application '{id}' was not found.");

        app.IsPublished = false;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(app);
    }

    public async Task<IReadOnlyList<ApplicationVersionDto>> ListVersionsAsync(Guid id, CancellationToken ct = default)
    {
        if (!await _db.Applications.AnyAsync(a => a.Id == id, ct))
            throw new KeyNotFoundException($"Application '{id}' was not found.");

        return await _db.ApplicationVersions
            .Where(v => v.ApplicationId == id)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new ApplicationVersionDto(v.Id, v.ApplicationId, v.VersionNumber, v.PublishedAt, v.CreatedAt))
            .ToListAsync(ct);
    }

    private static ApplicationDto ToDto(AppEntity a) =>
        new(a.Id, a.Name, a.Description, a.IsPublished, a.CurrentVersionId, a.CreatedAt, a.UpdatedAt);
}
