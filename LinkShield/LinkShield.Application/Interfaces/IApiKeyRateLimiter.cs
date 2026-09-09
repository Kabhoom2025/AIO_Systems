namespace LinkShield.Application.Interfaces;

/// <summary>
/// Per-minute request throttling for API keys (spec section 30's RequestsPerMinute, distinct
/// from the daily quota which IApiKeyValidator already enforces via a DB count). Deliberately
/// a singleton in-process fixed-window counter, not a distributed one — good enough for a
/// single-instance deployment; revisit with Redis if LinkShield.API ever runs more than one
/// replica, since counts would then be per-instance rather than global.
/// </summary>
public interface IApiKeyRateLimiter
{
    /// <returns>true if this request is allowed under the client's per-minute limit; false if
    /// the limit for the current one-minute window has already been reached.</returns>
    bool TryConsume(Guid apiClientId, int requestsPerMinute);
}
