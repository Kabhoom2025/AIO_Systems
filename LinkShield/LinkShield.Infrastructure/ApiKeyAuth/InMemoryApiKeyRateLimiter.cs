using System.Collections.Concurrent;
using LinkShield.Application.Interfaces;

namespace LinkShield.Infrastructure.ApiKeyAuth;

/// <summary>Fixed-window (not sliding-window) per-minute counter, keyed by API client and the
/// current UTC minute — simple, no background cleanup needed since old windows are just
/// abandoned dictionary entries that get replaced/overwritten as time moves on. A production
/// deployment with multiple LinkShield.API replicas would need this backed by Redis INCR/EXPIRE
/// instead, since this state is per-process.</summary>
public class InMemoryApiKeyRateLimiter : IApiKeyRateLimiter
{
    private readonly ConcurrentDictionary<(Guid ApiClientId, long WindowMinute), int> _counts = new();

    public bool TryConsume(Guid apiClientId, int requestsPerMinute)
    {
        var windowMinute = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute;
        var key = (apiClientId, windowMinute);

        var newCount = _counts.AddOrUpdate(key, 1, (_, existing) => existing + 1);

        if (newCount == 1)
        {
            // Cheap opportunistic cleanup of the previous window's entry so this dictionary
            // doesn't grow unbounded across long-running processes.
            _counts.TryRemove((apiClientId, windowMinute - 1), out _);
        }

        return newCount <= requestsPerMinute;
    }
}
