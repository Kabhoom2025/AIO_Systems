using AIO_Systems.Domain.Entities;

namespace AIO_Systems.Processes;

public interface IProcessOrchestratorService
{
    Task<AppStatus> GetStatusAsync(RegisteredService service);
    Task<AppStatus> StartAsync(RegisteredService service, RunMode mode);
    Task<AppStatus> StopAsync(RegisteredService service, RunMode mode);
}
