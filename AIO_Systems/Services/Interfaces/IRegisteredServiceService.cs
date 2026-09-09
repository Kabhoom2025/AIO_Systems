using AIO_Systems.DTOs.RegisteredService;
using AIO_Systems.Processes;

namespace AIO_Systems.Services.Interfaces;

public interface IRegisteredServiceService
{
    Task<List<RegisteredServiceDto>> GetAllAsync();
    Task<RegisteredServiceDto> GetByIdAsync(int id);
    Task<RegisteredServiceDto> CreateAsync(CreateRegisteredServiceDto dto);
    Task<RegisteredServiceDto> UpdateAsync(int id, UpdateRegisteredServiceDto dto);
    Task DeleteAsync(int id);
    Task<RegisteredServiceDto> StartAsync(int id, RunMode mode);
    Task<RegisteredServiceDto> StopAsync(int id, RunMode mode);
}
