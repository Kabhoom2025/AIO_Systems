using System.Diagnostics;
using FoodOrder.Application.DTOs.Backup;
using FoodOrder.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FoodOrder.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly string _backupDir;
    private readonly string _host;
    private readonly int    _port;
    private readonly string _database;
    private readonly string _username;
    private readonly string _password;
    private readonly string _pgDump;
    private readonly string _psql;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IConfiguration config, ILogger<BackupService> logger)
    {
        _logger    = logger;
        _backupDir = Path.Combine(AppContext.BaseDirectory, "Backups");
        Directory.CreateDirectory(_backupDir);

        var cs    = new NpgsqlConnectionStringBuilder(config.GetConnectionString("DefaultConnection") ?? "");
        _host     = cs.Host     ?? "localhost";
        _port     = cs.Port > 0 ? cs.Port : 5432;
        _database = cs.Database ?? "";
        _username = cs.Username ?? "postgres";
        _password = cs.Password ?? "";

        var toolsPath = config["Backup:PostgresToolsPath"]?.Trim();
        _pgDump = ResolveExe(toolsPath, "pg_dump");
        _psql   = ResolveExe(toolsPath, "psql");
    }

    // Resolve full path to a PostgreSQL tool, falling back to common install locations.
    private static string ResolveExe(string? configuredPath, string exe)
    {
        var exeName = OperatingSystem.IsWindows() ? exe + ".exe" : exe;

        // 1. Use path explicitly set in config.
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var full = Path.Combine(configuredPath, exeName);
            if (File.Exists(full)) return full;
        }

        // 2. Scan common Windows PostgreSQL install directories (versions 12–17).
        if (OperatingSystem.IsWindows())
        {
            var pgBase = @"C:\Program Files\PostgreSQL";
            if (Directory.Exists(pgBase))
            {
                var found = Directory.GetDirectories(pgBase)
                    .OrderByDescending(d => d)   // prefer latest version
                    .Select(d => Path.Combine(d, "bin", exeName))
                    .FirstOrDefault(File.Exists);
                if (found != null) return found;
            }
        }

        // 3. Fall back to bare command name and rely on the system PATH.
        return exe;
    }

    public Task<IEnumerable<BackupDto>> ListAsync()
    {
        var files = Directory.GetFiles(_backupDir, "*.sql")
            .Select(f =>
            {
                var info = new FileInfo(f);
                return new BackupDto
                {
                    FileName      = info.Name,
                    CreatedAt     = info.CreationTimeUtc,
                    SizeBytes     = info.Length,
                    SizeFormatted = FormatSize(info.Length),
                    Status        = info.Length > 0 ? "Success" : "Failed",
                };
            })
            .OrderByDescending(b => b.CreatedAt);

        return Task.FromResult<IEnumerable<BackupDto>>(files);
    }

    public async Task<BackupDto> CreateAsync()
    {
        var ts       = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"backup_{ts}.sql";
        var filePath = Path.Combine(_backupDir, fileName);

        var psi = new ProcessStartInfo
        {
            FileName               = _pgDump,
            Arguments              = $"-h {_host} -p {_port} -U {_username} -d {_database} -f \"{filePath}\" --no-password",
            RedirectStandardError  = true,
            RedirectStandardOutput = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };
        psi.Environment["PGPASSWORD"] = _password;

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException($"Could not start '{_pgDump}'. Ensure PostgreSQL client tools are installed and set Backup:PostgresToolsPath in appsettings.json.");

        var stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            _logger.LogError("pg_dump exited {Code}: {Error}", proc.ExitCode, stderr);
            if (File.Exists(filePath)) File.Delete(filePath);
            throw new InvalidOperationException($"Backup failed: {stderr.Trim()}");
        }

        var info = new FileInfo(filePath);
        return new BackupDto
        {
            FileName      = fileName,
            CreatedAt     = DateTime.UtcNow,
            SizeBytes     = info.Length,
            SizeFormatted = FormatSize(info.Length),
            Status        = "Success",
        };
    }

    public async Task<(byte[] Data, string FileName, string ContentType)> DownloadAsync(string fileName)
    {
        ValidateName(fileName);
        var path = Path.Combine(_backupDir, fileName);
        if (!File.Exists(path)) throw new FileNotFoundException("Backup file not found.", fileName);
        var data = await File.ReadAllBytesAsync(path);
        return (data, fileName, "application/octet-stream");
    }

    public async Task RestoreAsync(string fileName)
    {
        ValidateName(fileName);
        var filePath = Path.Combine(_backupDir, fileName);
        if (!File.Exists(filePath)) throw new FileNotFoundException("Backup file not found.", fileName);

        var psi = new ProcessStartInfo
        {
            FileName               = _psql,
            Arguments              = $"-h {_host} -p {_port} -U {_username} -d {_database} -f \"{filePath}\" --no-password",
            RedirectStandardError  = true,
            RedirectStandardOutput = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };
        psi.Environment["PGPASSWORD"] = _password;

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException($"Could not start '{_psql}'. Ensure PostgreSQL client tools are installed and set Backup:PostgresToolsPath in appsettings.json.");

        var stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
        {
            _logger.LogError("psql restore exited {Code}: {Error}", proc.ExitCode, stderr);
            throw new InvalidOperationException($"Restore failed: {stderr.Trim()}");
        }
    }

    public Task DeleteAsync(string fileName)
    {
        ValidateName(fileName);
        var path = Path.Combine(_backupDir, fileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private static void ValidateName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Contains('/')
            || fileName.Contains('\\')
            || fileName.Contains(".."))
            throw new ArgumentException("Invalid backup file name.", nameof(fileName));
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F1} GB",
        >= 1_048_576     => $"{bytes / 1_048_576.0:F1} MB",
        >= 1_024         => $"{bytes / 1_024.0:F1} KB",
        _                => $"{bytes} B",
    };
}
