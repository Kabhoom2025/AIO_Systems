using LinkShield.Domain.Common;
using LinkShield.Domain.Enums;

namespace LinkShield.Domain.Entities;

public class ScanEvent : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public ScanStatus Stage { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
