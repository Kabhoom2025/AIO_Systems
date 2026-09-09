using FoodOrder.Application.Interfaces.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.API.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/backup")]
public class BackupController(IBackupService backupService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
        => Ok(ApiResponse<object>.SuccessResult(await backupService.ListAsync()));

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var result = await backupService.CreateAsync();
        return Ok(ApiResponse<object>.SuccessResult(result, "Backup created successfully."));
    }

    [HttpGet("{fileName}/download")]
    public async Task<IActionResult> Download(string fileName)
    {
        var (data, name, contentType) = await backupService.DownloadAsync(fileName);
        return File(data, contentType, name);
    }

    [HttpPost("{fileName}/restore")]
    public async Task<IActionResult> Restore(string fileName)
    {
        await backupService.RestoreAsync(fileName);
        return Ok(ApiResponse<object>.SuccessResult(null, "Database restored successfully."));
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> Delete(string fileName)
    {
        await backupService.DeleteAsync(fileName);
        return NoContent();
    }
}
