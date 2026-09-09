using LinkShield.Domain.Enums;

namespace LinkShield.Application.Interfaces;

/// <summary>Runs every analysis stage after URL analysis (which SubmitScanAsync already runs
/// synchronously) for one scan: domain/RDAP, DNS, SSL, redirects, threat intelligence, brand
/// detection, ML, then risk scoring. Never throws — a stage failure is recorded on the scan
/// (Status=Failed, FailureReason) rather than propagating, since a background queue has no
/// caller to propagate to.</summary>
public interface IScanPipelineRunner
{
    Task RunAsync(Guid scanId, CancellationToken ct = default);
}

/// <summary>In-process queue handing scan ids from the API request thread to the background
/// processing hosted service, so a scan's real network calls never block the HTTP response
/// (spec section 17: "Long-running scans must not block normal API requests").</summary>
public interface IScanQueue
{
    void Enqueue(Guid scanId);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct);
}

/// <summary>Broadcasts stage-transition events for a scan (spec section 20). Implemented in
/// LinkShield.API against ScanProgressHub — Infrastructure only depends on this interface.</summary>
public interface IScanNotifier
{
    Task NotifyStageAsync(Guid scanId, ScanStatus stage, string message, CancellationToken ct = default);
}
