using LinkShield.Application.Interfaces;
using LinkShield.Domain.Enums;

namespace LinkShield.Infrastructure.BackgroundProcessing;

/// <summary>Default no-op registration — LinkShield.API overrides this with a SignalR-backed
/// implementation after calling AddLinkShieldInfrastructure. Anything that resolves
/// IScanNotifier without that override (e.g. LinkShield.Worker) still works, it just doesn't
/// broadcast anywhere.</summary>
public class NullScanNotifier : IScanNotifier
{
    public Task NotifyStageAsync(Guid scanId, ScanStatus stage, string message, CancellationToken ct = default) => Task.CompletedTask;
}
