namespace AIO_Systems.Domain.Entities;

/// <summary>
/// Service registry entry — lets the super admin register/manage other backend microservices
/// (e.g. FoodOrder.API, Pharmacy.API) that trust JWTs minted by AIO_Systems. Also doubles as
/// the Apps launcher's source of truth: when the launch fields below are filled in, the Apps
/// page can start/stop this service's backend and frontend as local OS processes.
/// </summary>
public class RegisteredService : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>CSV of PlatformModule.Key values this service serves, e.g. "pharmacy" or "hr_management,finance".</summary>
    public string ModuleKeys { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? HealthCheckPath { get; set; }
    public DateTime? LastVerifiedAt { get; set; }

    // ── Apps launcher fields (all optional — a routing/health-only entry can leave these blank) ──
    public string? BackendWorkingDirectory { get; set; }
    public string? BackendCommand { get; set; } // e.g. "dotnet run"

    /// <summary>Mirrors BaseUrl's shape for the frontend, e.g. "http://localhost:4200".</summary>
    public string? FrontendUrl { get; set; }
    public string? FrontendWorkingDirectory { get; set; }
    public string? FrontendCommand { get; set; } // e.g. "npm start"

    /// <summary>
    /// The service name for this backend in the repo-root docker-compose.yml (e.g. "hrms-api").
    /// Lets the Apps page start/stop the backend as a Docker container instead of a native
    /// process. Left blank, only Native mode is available for this app. There's no per-app
    /// frontend container in this repo, so Docker mode only ever manages the backend — the
    /// frontend still needs `npm start` natively either way.
    /// </summary>
    public string? DockerServiceName { get; set; }

    public string Icon { get; set; } = "dns";
    public string Color { get; set; } = "#6c757d";
    public int SortOrder { get; set; }
}
