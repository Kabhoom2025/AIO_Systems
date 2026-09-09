namespace AIO_Systems.DTOs.RegisteredService;

public class RegisteredServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ModuleKeys { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? HealthCheckPath { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public DateTime CreatedDate { get; set; }

    public string? BackendWorkingDirectory { get; set; }
    public string? BackendCommand { get; set; }
    public bool BackendRunning { get; set; }

    public string? FrontendUrl { get; set; }
    public string? FrontendWorkingDirectory { get; set; }
    public string? FrontendCommand { get; set; }
    public bool FrontendRunning { get; set; }

    public string? DockerServiceName { get; set; }

    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CreateRegisteredServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ModuleKeys { get; set; } = string.Empty;
    public string? HealthCheckPath { get; set; }

    public string? BackendWorkingDirectory { get; set; }
    public string? BackendCommand { get; set; }
    public string? FrontendUrl { get; set; }
    public string? FrontendWorkingDirectory { get; set; }
    public string? FrontendCommand { get; set; }
    public string? DockerServiceName { get; set; }

    public string Icon { get; set; } = "dns";
    public string Color { get; set; } = "#6c757d";
    public int SortOrder { get; set; }
}

public class UpdateRegisteredServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ModuleKeys { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? HealthCheckPath { get; set; }

    public string? BackendWorkingDirectory { get; set; }
    public string? BackendCommand { get; set; }
    public string? FrontendUrl { get; set; }
    public string? FrontendWorkingDirectory { get; set; }
    public string? FrontendCommand { get; set; }
    public string? DockerServiceName { get; set; }

    public string Icon { get; set; } = "dns";
    public string Color { get; set; } = "#6c757d";
    public int SortOrder { get; set; }
}
