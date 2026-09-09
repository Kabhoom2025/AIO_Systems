using Microsoft.EntityFrameworkCore;
using Platform.Application.ApiDesigner;
using Platform.Infrastructure.Persistence;
using ServiceEntity = Platform.Domain.Entities.ServiceDefinition;

namespace Platform.Infrastructure.ApiDesigner;

public class ServiceDefinitionService : IServiceDefinitionService
{
    private readonly PlatformDbContext _db;

    public ServiceDefinitionService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ServiceDto>> ListAsync(Guid applicationId, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        return await _db.Services
            .Where(s => s.ApplicationId == applicationId)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(s.Id, s.ApplicationId, s.Name, s.TableId, s.Description, s.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ServiceDto> GetAsync(Guid applicationId, Guid serviceId, CancellationToken ct = default)
    {
        var service = await LoadAsync(applicationId, serviceId, ct);
        return ToDto(service);
    }

    public async Task<ServiceDto> CreateAsync(Guid applicationId, CreateServiceRequest request, CancellationToken ct = default)
    {
        await EnsureApplicationExistsAsync(applicationId, ct);

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.");

        if (await _db.Services.AnyAsync(s => s.ApplicationId == applicationId && s.Name == name, ct))
            throw new InvalidOperationException($"A service named '{name}' already exists in this application.");

        if (request.TableId is Guid tableId && !await _db.Tables.AnyAsync(t => t.ApplicationId == applicationId && t.Id == tableId, ct))
            throw new ArgumentException($"Table '{tableId}' was not found in this application.");

        var service = new ServiceEntity
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = name,
            TableId = request.TableId,
            Description = request.Description?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync(ct);
        return ToDto(service);
    }

    public async Task<ServiceDto> UpdateAsync(Guid applicationId, Guid serviceId, UpdateServiceRequest request, CancellationToken ct = default)
    {
        var service = await LoadAsync(applicationId, serviceId, ct);

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.");

        if (!string.Equals(service.Name, name, StringComparison.Ordinal) &&
            await _db.Services.AnyAsync(s => s.ApplicationId == applicationId && s.Name == name && s.Id != serviceId, ct))
            throw new InvalidOperationException($"A service named '{name}' already exists in this application.");

        if (request.TableId is Guid tableId && !await _db.Tables.AnyAsync(t => t.ApplicationId == applicationId && t.Id == tableId, ct))
            throw new ArgumentException($"Table '{tableId}' was not found in this application.");

        service.Name = name;
        service.TableId = request.TableId;
        service.Description = request.Description?.Trim();

        await _db.SaveChangesAsync(ct);
        return ToDto(service);
    }

    public async Task DeleteAsync(Guid applicationId, Guid serviceId, CancellationToken ct = default)
    {
        var service = await LoadAsync(applicationId, serviceId, ct);
        _db.Services.Remove(service);
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureApplicationExistsAsync(Guid applicationId, CancellationToken ct)
    {
        if (!await _db.Applications.AnyAsync(a => a.Id == applicationId, ct))
            throw new KeyNotFoundException($"Application '{applicationId}' was not found.");
    }

    private async Task<ServiceEntity> LoadAsync(Guid applicationId, Guid serviceId, CancellationToken ct)
    {
        return await _db.Services.FirstOrDefaultAsync(s => s.ApplicationId == applicationId && s.Id == serviceId, ct)
            ?? throw new KeyNotFoundException($"Service '{serviceId}' was not found.");
    }

    private static ServiceDto ToDto(ServiceEntity s) => new(s.Id, s.ApplicationId, s.Name, s.TableId, s.Description, s.CreatedAt);
}
