using LinkShield.Application.Interfaces;

namespace LinkShield.API.BackgroundServices;

/// <summary>
/// Drains IScanQueue and runs IScanPipelineRunner for each scan id, in-process, so the domain/
/// DNS/SSL/redirect/threat-intel/brand/ML/risk-scoring stages (all real network calls) never
/// block the request thread that handled POST /api/v1/scans. Each item gets its own DI scope
/// since the pipeline runner depends on scoped services (DbContext, analyzers).
/// </summary>
public class ScanProcessingHostedService : BackgroundService
{
    private readonly IScanQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScanProcessingHostedService> _logger;

    public ScanProcessingHostedService(IScanQueue queue, IServiceScopeFactory scopeFactory, ILogger<ScanProcessingHostedService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var scanId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<IScanPipelineRunner>();
                await runner.RunAsync(scanId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // The pipeline runner already catches and records stage failures on the scan
                // itself; reaching here means something broke outside that (e.g. DI scope
                // creation) — log and keep draining the queue rather than crashing the host.
                _logger.LogError(ex, "Unhandled error processing scan {ScanId}", scanId);
            }
        }
    }
}
