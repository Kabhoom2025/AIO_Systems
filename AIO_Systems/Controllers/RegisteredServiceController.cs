using AIO_Systems.DTOs.RegisteredService;
using AIO_Systems.Processes;
using AIO_Systems.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIO_Systems.Controllers;

[Authorize(Roles = "SuperAdmin")]
[Route("api/registered-services")]
public class RegisteredServiceController(IRegisteredServiceService registeredServiceService) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await registeredServiceService.GetAllAsync();
        return Ok(services);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await registeredServiceService.GetByIdAsync(id);
        return Ok(service);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegisteredServiceDto dto)
    {
        var service = await registeredServiceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRegisteredServiceDto dto)
    {
        var service = await registeredServiceService.UpdateAsync(id, dto);
        return Ok(service);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await registeredServiceService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id, [FromQuery] RunMode mode = RunMode.Native)
    {
        var service = await registeredServiceService.StartAsync(id, mode);
        return Ok(service);
    }

    [HttpPost("{id:int}/stop")]
    public async Task<IActionResult> Stop(int id, [FromQuery] RunMode mode = RunMode.Native)
    {
        var service = await registeredServiceService.StopAsync(id, mode);
        return Ok(service);
    }
}
