using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Linq;
using AIO_Systems.Domain.Entities;
using AIO_Systems.Shared;

namespace AIO_Systems.Processes;

/// <summary>
/// Starts/stops a registered service's backend and frontend as real OS processes on the
/// machine AIO_Systems itself runs on. This is a local-dev convenience, not something
/// that would make sense against a remote/hosted deployment. Ports are derived from
/// BaseUrl/FrontendUrl rather than stored separately, so there's exactly one place a
/// port is ever entered (the URL) — no drift between "the URL" and "the port field."
/// </summary>
public class ProcessOrchestratorService(ILogger<ProcessOrchestratorService> logger, IConfiguration config) : IProcessOrchestratorService
{
    // Where the repo-root docker-compose.yml lives — one compose file serves every backend,
    // so this is a single config value rather than a per-service field.
    private string DockerComposeWorkingDirectory =>
        config["DockerCompose:WorkingDirectory"] ?? throw new AppException(
            "DockerCompose:WorkingDirectory is not configured in appsettings.json.", 500);

    // Tracks PIDs we ourselves spawned, keyed "{serviceId}:backend" / "{serviceId}:frontend".
    // Purely a fast-path — Stop falls back to a netstat-based port lookup so it still
    // works after an AIO_Systems restart or if the process was started manually.
    private static readonly ConcurrentDictionary<string, int> SpawnedPids = new();

    // Serializes start/stop per half so two near-simultaneous "Run" clicks (or a click
    // landing while a previous launch is still compiling) can't both pass the "is it
    // running yet" check and spawn two processes racing for the same port.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public async Task<AppStatus> GetStatusAsync(RegisteredService service)
    {
        var backendPort = TryGetPort(service.BaseUrl);
        var frontendPort = TryGetPort(service.FrontendUrl);

        // Backend and frontend probes are independent — run them concurrently instead of
        // one-after-the-other. With both down this halves the worst case from ~1s to ~0.5s
        // per service, which matters a lot once GetAllAsync fans this out across every
        // registered service (see RegisteredServiceService.GetAllAsync).
        var backendTask = backendPort.HasValue ? IsPortOpenAsync(backendPort.Value) : Task.FromResult(false);
        var frontendTask = frontendPort.HasValue ? IsPortOpenAsync(frontendPort.Value) : Task.FromResult(false);
        await Task.WhenAll(backendTask, frontendTask);
        return new AppStatus { BackendRunning = backendTask.Result, FrontendRunning = frontendTask.Result };
    }

    public async Task<AppStatus> StartAsync(RegisteredService service, RunMode mode)
    {
        EnsureCanLaunchProcesses();

        var backendKey = Key(service.Id, "backend");
        var frontendKey = Key(service.Id, "frontend");
        var backendPort = TryGetPort(service.BaseUrl);
        var frontendPort = TryGetPort(service.FrontendUrl);

        // Guard against double-launch: a dev server can take 15-30s to bind its port
        // (still compiling), so "port not open yet" does NOT mean "safe to launch
        // another one" — clicking Run again mid-startup would otherwise spawn a second
        // process racing for the same port and crash with "address already in use".
        // The per-key lock makes the check-then-launch atomic across concurrent requests.
        if (mode == RunMode.Docker)
        {
            if (string.IsNullOrWhiteSpace(service.DockerServiceName))
                throw new AppException($"'{service.Name}' has no Docker Compose service name configured — set one before using Docker mode.", 400);

            if (backendPort.HasValue)
            {
                await WithLockAsync(backendKey, async () =>
                {
                    if (!await IsPortOpenAsync(backendPort.Value) && !IsTrackedProcessAlive(backendKey))
                        LaunchDocker(service.DockerServiceName!, backendKey);
                });
            }
        }
        else if (backendPort.HasValue && !string.IsNullOrWhiteSpace(service.BackendWorkingDirectory)
            && !string.IsNullOrWhiteSpace(service.BackendCommand))
        {
            await WithLockAsync(backendKey, async () =>
            {
                if (!await IsPortOpenAsync(backendPort.Value) && !IsTrackedProcessAlive(backendKey))
                    Launch(service.BackendWorkingDirectory!, service.BackendCommand!, backendKey);
            });
        }

        // The frontend has no Docker container in this repo regardless of the selected
        // mode — it always launches natively via npm/ng.
        if (frontendPort.HasValue && !string.IsNullOrWhiteSpace(service.FrontendWorkingDirectory)
            && !string.IsNullOrWhiteSpace(service.FrontendCommand))
        {
            await WithLockAsync(frontendKey, async () =>
            {
                if (!await IsPortOpenAsync(frontendPort.Value) && !IsTrackedProcessAlive(frontendKey))
                    Launch(service.FrontendWorkingDirectory!, service.FrontendCommand!, frontendKey);
            });
        }

        return await GetStatusAsync(service);
    }

    public async Task<AppStatus> StopAsync(RegisteredService service, RunMode mode)
    {
        EnsureCanLaunchProcesses();

        var backendPort = TryGetPort(service.BaseUrl);
        var frontendPort = TryGetPort(service.FrontendUrl);

        if (backendPort.HasValue)
        {
            if (mode == RunMode.Docker && !string.IsNullOrWhiteSpace(service.DockerServiceName))
                await StopDockerAsync(service.DockerServiceName!, Key(service.Id, "backend"));
            else
                await KillAsync(backendPort.Value, Key(service.Id, "backend"));
        }

        if (frontendPort.HasValue)
            await KillAsync(frontendPort.Value, Key(service.Id, "frontend"));

        return await GetStatusAsync(service);
    }

    // The launcher spawns real OS processes (cmd.exe, taskkill, netstat) using Windows host
    // paths from RegisteredService.BackendWorkingDirectory/FrontendWorkingDirectory. That's
    // structurally impossible from inside a Linux container — there's no such filesystem, no
    // such shell, and usually not even the .NET/Node SDKs needed to run the target project.
    // Fail fast with a clear message instead of a raw 500 from a DirectoryNotFoundException.
    private static void EnsureCanLaunchProcesses()
    {
        if (!OperatingSystem.IsWindows() || File.Exists("/.dockerenv"))
            throw new AppException(
                "The Apps launcher spawns local Windows processes and can't run from inside this container. " +
                "Run AIO_Systems natively (dotnet run) on the host instead of via Docker to use Start/Stop.",
                statusCode: 409);
    }

    private static int? TryGetPort(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Port : null;
    }

    private static async Task WithLockAsync(string key, Func<Task> action)
    {
        var gate = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            gate.Release();
        }
    }

    private static bool IsTrackedProcessAlive(string trackingKey)
    {
        if (!SpawnedPids.TryGetValue(trackingKey, out var pid))
            return false;

        try
        {
            using var process = Process.GetProcessById(pid);
            if (!process.HasExited)
                return true;
        }
        catch (ArgumentException)
        {
            // Process no longer exists.
        }

        SpawnedPids.TryRemove(trackingKey, out _);
        return false;
    }

    private void Launch(string workingDirectory, string command, string trackingKey)
    {
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException($"Working directory not found: {workingDirectory}");

        var process = StartShellCommand(command, workingDirectory, waitForExit: false)
            ?? throw new InvalidOperationException($"Failed to start process for command: {command}");

        SpawnedPids[trackingKey] = process.Id;
        logger.LogInformation("Started '{Command}' in {Dir} — PID {Pid}", command, workingDirectory, process.Id);
    }

    // Docker containers keep running after `docker compose up -d` exits, so — unlike native
    // Launch — there's no long-lived PID to track: we let the compose command finish and rely
    // entirely on the port probe (IsPortOpenAsync) for status, same as GetStatusAsync always has.
    private void LaunchDocker(string dockerServiceName, string trackingKey)
    {
        var workingDirectory = DockerComposeWorkingDirectory;
        if (!Directory.Exists(workingDirectory))
            throw new DirectoryNotFoundException($"Docker Compose working directory not found: {workingDirectory}");

        SpawnedPids.TryRemove(trackingKey, out _);
        var process = StartShellCommand($"docker compose up -d --build {dockerServiceName}", workingDirectory, waitForExit: false)
            ?? throw new InvalidOperationException($"Failed to start docker compose for service: {dockerServiceName}");

        logger.LogInformation("Launched 'docker compose up -d --build {Service}' — PID {Pid}", dockerServiceName, process.Id);
    }

    private async Task StopDockerAsync(string dockerServiceName, string trackingKey)
    {
        SpawnedPids.TryRemove(trackingKey, out _);
        var workingDirectory = DockerComposeWorkingDirectory;

        var process = StartShellCommand($"docker compose stop {dockerServiceName}", workingDirectory, waitForExit: true);
        if (process is not null) await process.WaitForExitAsync();

        logger.LogInformation("Stopped docker compose service '{Service}'", dockerServiceName);
    }

    private static Process? StartShellCommand(string command, string workingDirectory, bool waitForExit) =>
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = waitForExit,
            RedirectStandardError = waitForExit,
        });

    private async Task KillAsync(int port, string trackingKey)
    {
        var pid = SpawnedPids.TryGetValue(trackingKey, out var trackedPid)
            ? trackedPid
            : await FindPidListeningOnPortAsync(port);

        SpawnedPids.TryRemove(trackingKey, out _);

        if (pid is null)
            return;

        try
        {
            var kill = Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/PID {pid} /T /F",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (kill is not null)
                await kill.WaitForExitAsync();
            logger.LogInformation("Stopped PID {Pid} (port {Port})", pid, port);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to kill PID {Pid} on port {Port}", pid, port);
        }
    }

    // Different dev servers bind loopback differently — Kestrel (dotnet run) is
    // typically dual-stack, but `ng serve` binds IPv6-only ([::1]). Probing only
    // 127.0.0.1 silently misreports a healthy Angular server as "not running".
    private static readonly string[] LoopbackHosts = ["127.0.0.1", "::1"];

    private static async Task<bool> IsPortOpenAsync(int port)
    {
        // Probe both loopback hosts concurrently rather than one-after-the-other — a
        // closed port on either can take the full timeout to fail, so sequential probing
        // doubled the worst case for no benefit (we only care whether EITHER is open).
        var tasks = LoopbackHosts.Select(host => TryConnectAsync(host, port)).ToArray();
        var results = await Task.WhenAll(tasks);
        return results.Any(open => open);
    }

    private static async Task<bool> TryConnectAsync(string host, int port)
    {
        // A real CancellationToken (not just racing a Task.Delay via WhenAny) actually
        // aborts the in-flight connect attempt once we give up on it — otherwise the
        // abandoned ConnectAsync keeps running against the OS's own (much longer) connect
        // timeout in the background, and enough of those piling up across repeated polls
        // is exactly what could turn an occasional probe into the ~59s hang seen in
        // production instead of failing fast like every other probe.
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cts.Token);
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<int?> FindPidListeningOnPortAsync(int port)
    {
        try
        {
            // No "-p tcp" filter: on Windows that silently excludes IPv6-only
            // listeners (e.g. `ng serve` on [::1]), making this lookup return
            // nothing for those processes. Plain "-ano" lists both and the regex
            // below already filters to LISTENING rows on the target port.
            var startInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process is null) return null;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var pattern = new Regex($@"^\s*TCP\s+\S*:{port}\s+\S+\s+LISTENING\s+(\d+)\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var match = pattern.Match(output);
            return match.Success ? int.Parse(match.Groups[1].Value) : null;
        }
        catch
        {
            return null;
        }
    }

    private static string Key(int serviceId, string half) => $"{serviceId}:{half}";
}
