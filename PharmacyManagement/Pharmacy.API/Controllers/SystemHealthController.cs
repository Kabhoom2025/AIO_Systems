using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.API.DTOs;
using Pharmacy.Infrastructure.Data;

namespace Pharmacy.API.Controllers;

[ApiController]
[Route("api/system-health")]
[AllowAnonymous] // monitored by the Super Admin dashboard; data is non-sensitive
public class SystemHealthController(PharmacyDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var proc = Process.GetCurrentProcess();

        var dbStatus = "Connected";
        long dbLatencyMs = 0;
        try
        {
            var sw = Stopwatch.StartNew();
            _ = await db.Medicines.CountAsync();
            sw.Stop();
            dbLatencyMs = sw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            dbStatus = $"Error: {ex.Message[..Math.Min(ex.Message.Length, 80)]}";
        }

        proc.Refresh();
        var uptime = DateTime.UtcNow - proc.StartTime.ToUniversalTime();

        var dto = new PharmacyHealthDto
        {
            ApiStatus = "Online",
            DbStatus = dbStatus,
            DbLatencyMs = dbLatencyMs,
            MemoryUsedMb = Math.Round(proc.WorkingSet64 / 1_048_576.0, 1),
            Uptime = uptime.TotalDays >= 1
                ? $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m"
                : $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
            MachineName = Environment.MachineName,
            DotNetVersion = RuntimeInformation.FrameworkDescription,
        };

        return Ok(dto);
    }
}
