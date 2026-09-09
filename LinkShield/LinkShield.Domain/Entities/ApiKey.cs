using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class ApiKey : BaseEntity
{
    public Guid ApiClientId { get; set; }
    public ApiClient ApiClient { get; set; } = null!;

    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
    public long UsageCount { get; set; }

    public bool IsActive => RevokedAtUtc is null && (ExpiresAtUtc is null || DateTime.UtcNow < ExpiresAtUtc);
}
