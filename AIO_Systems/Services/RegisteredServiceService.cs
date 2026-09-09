using AIO_Systems.Data;
using AIO_Systems.DTOs.RegisteredService;
using AIO_Systems.Processes;
using AIO_Systems.Services.Interfaces;
using AIO_Systems.Shared;
using Microsoft.EntityFrameworkCore;

namespace AIO_Systems.Services;

public class RegisteredServiceService(AppDbContext db, IProcessOrchestratorService orchestrator) : IRegisteredServiceService
{
    public async Task<List<RegisteredServiceDto>> GetAllAsync()
    {
        var list = await db.RegisteredServices.OrderBy(s => s.SortOrder).ToListAsync();

        // Status checks are pure TCP port probes against other processes — nothing here
        // touches the DbContext, so they're safe to fan out concurrently. This used to be
        // a sequential foreach/await, which meant this endpoint's latency scaled linearly
        // with the number of registered services (each one adding its own probe timeout
        // whenever that service happened to be down). With N services down, that easily
        // exceeded the Apps page's 4s poll interval, causing every request to get
        // cancelled by the next poll tick before it could ever finish.
        var statusTasks = list.Select(orchestrator.GetStatusAsync).ToArray();
        var statuses = await Task.WhenAll(statusTasks);

        return list.Zip(statuses, ToDto).ToList();
    }

    public async Task<RegisteredServiceDto> GetByIdAsync(int id)
    {
        var entity = await GetEntityAsync(id);
        var status = await orchestrator.GetStatusAsync(entity);
        return ToDto(entity, status);
    }

    public async Task<RegisteredServiceDto> CreateAsync(CreateRegisteredServiceDto dto)
    {
        if (await db.RegisteredServices.AnyAsync(s => s.RoutePrefix == dto.RoutePrefix))
            throw new AppException($"A service with route prefix '{dto.RoutePrefix}' already exists.", 400);

        ValidateUrl(dto.BaseUrl, "Backend URL", required: true);
        ValidateUrl(dto.FrontendUrl, "Frontend URL", required: false);

        var entity = new Domain.Entities.RegisteredService
        {
            Name = dto.Name,
            RoutePrefix = dto.RoutePrefix,
            BaseUrl = dto.BaseUrl,
            Description = dto.Description,
            ModuleKeys = dto.ModuleKeys,
            HealthCheckPath = dto.HealthCheckPath,
            IsActive = true,
            BackendWorkingDirectory = dto.BackendWorkingDirectory,
            BackendCommand = dto.BackendCommand,
            FrontendUrl = dto.FrontendUrl,
            FrontendWorkingDirectory = dto.FrontendWorkingDirectory,
            FrontendCommand = dto.FrontendCommand,
            DockerServiceName = dto.DockerServiceName,
            Icon = dto.Icon,
            Color = dto.Color,
            SortOrder = dto.SortOrder,
        };

        db.RegisteredServices.Add(entity);
        await db.SaveChangesAsync();
        return ToDto(entity, new AppStatus());
    }

    public async Task<RegisteredServiceDto> UpdateAsync(int id, UpdateRegisteredServiceDto dto)
    {
        var entity = await GetEntityAsync(id);

        if (await db.RegisteredServices.AnyAsync(s => s.RoutePrefix == dto.RoutePrefix && s.Id != id))
            throw new AppException($"A service with route prefix '{dto.RoutePrefix}' already exists.", 400);

        ValidateUrl(dto.BaseUrl, "Backend URL", required: true);
        ValidateUrl(dto.FrontendUrl, "Frontend URL", required: false);

        // Snapshot the URLs as they were before this edit, so — if the port actually
        // changed — we can stop whatever process is still bound to the OLD port. Once
        // the port field changes, that process is orphaned: health checks and future
        // Start/Stop calls will only ever look at the NEW port, so the old one would
        // otherwise keep running invisibly, using stale/incorrect configuration.
        var oldBaseUrl = entity.BaseUrl;
        var oldFrontendUrl = entity.FrontendUrl;
        var backendPortChanged = TryGetPort(oldBaseUrl) != TryGetPort(dto.BaseUrl);
        var frontendPortChanged = TryGetPort(oldFrontendUrl) != TryGetPort(dto.FrontendUrl);

        entity.Name = dto.Name;
        entity.RoutePrefix = dto.RoutePrefix;
        entity.BaseUrl = dto.BaseUrl;
        entity.Description = dto.Description;
        entity.ModuleKeys = dto.ModuleKeys;
        entity.IsActive = dto.IsActive;
        entity.HealthCheckPath = dto.HealthCheckPath;
        entity.BackendWorkingDirectory = dto.BackendWorkingDirectory;
        entity.BackendCommand = dto.BackendCommand;
        entity.FrontendUrl = dto.FrontendUrl;
        entity.FrontendWorkingDirectory = dto.FrontendWorkingDirectory;
        entity.FrontendCommand = dto.FrontendCommand;
        entity.DockerServiceName = dto.DockerServiceName;
        entity.Icon = dto.Icon;
        entity.Color = dto.Color;
        entity.SortOrder = dto.SortOrder;

        await db.SaveChangesAsync();

        if (backendPortChanged || frontendPortChanged)
        {
            var staleSnapshot = new Domain.Entities.RegisteredService
            {
                Id = entity.Id,
                BaseUrl = backendPortChanged ? oldBaseUrl : string.Empty,
                FrontendUrl = frontendPortChanged ? oldFrontendUrl : null,
            };
            // We don't persist which mode a still-running process was started in, so this
            // cleanup always uses the port-based native kill — which also works for a
            // Docker container's published host port (it kills the docker-proxy binding).
            await orchestrator.StopAsync(staleSnapshot, RunMode.Native);
        }

        var status = await orchestrator.GetStatusAsync(entity);
        return ToDto(entity, status);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetEntityAsync(id);
        db.RegisteredServices.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task<RegisteredServiceDto> StartAsync(int id, RunMode mode)
    {
        var entity = await GetEntityAsync(id);
        var status = await orchestrator.StartAsync(entity, mode);
        return ToDto(entity, status);
    }

    public async Task<RegisteredServiceDto> StopAsync(int id, RunMode mode)
    {
        var entity = await GetEntityAsync(id);
        var status = await orchestrator.StopAsync(entity, mode);
        return ToDto(entity, status);
    }

    private async Task<Domain.Entities.RegisteredService> GetEntityAsync(int id) =>
        await db.RegisteredServices.FindAsync(id) ?? throw new AppException("Registered service not found.", 404);

    private static void ValidateUrl(string? url, string fieldName, bool required)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            if (required)
                throw new AppException($"{fieldName} is required.", 400);
            return;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new AppException($"{fieldName} must be a valid URL, e.g. http://localhost:5275.", 400);

        if (uri.IsDefaultPort)
            throw new AppException($"{fieldName} must include an explicit port, e.g. http://localhost:5275.", 400);
    }

    private static int? TryGetPort(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Port : null;
    }

    private static RegisteredServiceDto ToDto(Domain.Entities.RegisteredService s, AppStatus status) => new()
    {
        Id = s.Id,
        Name = s.Name,
        RoutePrefix = s.RoutePrefix,
        BaseUrl = s.BaseUrl,
        Description = s.Description,
        ModuleKeys = s.ModuleKeys,
        IsActive = s.IsActive,
        HealthCheckPath = s.HealthCheckPath,
        LastVerifiedAt = s.LastVerifiedAt,
        CreatedDate = s.CreatedDate,
        BackendWorkingDirectory = s.BackendWorkingDirectory,
        BackendCommand = s.BackendCommand,
        BackendRunning = status.BackendRunning,
        FrontendUrl = s.FrontendUrl,
        FrontendWorkingDirectory = s.FrontendWorkingDirectory,
        FrontendCommand = s.FrontendCommand,
        FrontendRunning = status.FrontendRunning,
        DockerServiceName = s.DockerServiceName,
        Icon = s.Icon,
        Color = s.Color,
        SortOrder = s.SortOrder,
    };
}
