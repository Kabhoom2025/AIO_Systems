using FoodOrder.Application.DTOs.SystemHealth;
using FoodOrder.Infrastructure.Data;
using FoodOrder.Infrastructure.Services;
using FoodOrder.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FoodOrder.API.Controllers;

[ApiController]
[Route("api/system-health")]
[AllowAnonymous]   // SA portal uses its own auth guard; endpoint data is non-sensitive
public class SystemHealthController : ControllerBase
{
    private readonly AppDbContext    _db;
    private readonly EventLogService _log;

    public SystemHealthController(AppDbContext db, EventLogService log)
    {
        _db  = db;
        _log = log;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SystemHealthDto>>> Get()
    {
        var proc = Process.GetCurrentProcess();

        // ── Database probe ────────────────────────────────────────────────────
        var dbStatus = "Connected";
        long dbLatencyMs = 0;
        try
        {
            var sw = Stopwatch.StartNew();
            _ = await _db.Users.CountAsync();
            sw.Stop();
            dbLatencyMs = sw.ElapsedMilliseconds;

            if (dbLatencyMs > 500)
                _log.Log("WARN", $"DB query took {dbLatencyMs} ms — above 500 ms threshold.", "Database");
        }
        catch (Exception ex)
        {
            dbStatus = $"Error: {ex.Message[..Math.Min(ex.Message.Length, 80)]}";
            _log.Log("ERROR", $"DB health probe failed: {ex.Message}", "Database");
        }

        // ── Process memory ────────────────────────────────────────────────────
        proc.Refresh();
        var memUsedMb = Math.Round(proc.WorkingSet64   / 1_048_576.0, 1);
        var gcHeapMb  = Math.Round(GC.GetTotalMemory(false) / 1_048_576.0, 1);

        // ── Uptime ────────────────────────────────────────────────────────────
        var uptime = DateTime.UtcNow - proc.StartTime.ToUniversalTime();
        var uptimeStr = uptime.TotalDays >= 1
            ? $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m"
            : $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";

        var dto = new SystemHealthDto
        {
            ApiStatus     = "Online",
            DbStatus      = dbStatus,
            DbLatencyMs   = dbLatencyMs,
            MemoryUsedMb  = memUsedMb,
            GcHeapMb      = gcHeapMb,
            Uptime        = uptimeStr,
            MachineName   = Environment.MachineName,
            OsDescription = RuntimeInformation.OSDescription,
            DotNetVersion = RuntimeInformation.FrameworkDescription,
            ProcessId     = proc.Id,
            ThreadCount   = proc.Threads.Count,
            RecentEvents  = [.. _log.GetRecent(25)],
        };

        return Ok(ApiResponse<SystemHealthDto>.SuccessResult(dto, "OK"));
    }
}
