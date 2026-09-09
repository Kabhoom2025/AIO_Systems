using LinkShield.Domain.Common;
using LinkShield.Domain.Enums;

namespace LinkShield.Domain.Entities;

public class UrlScan : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? ApiClientId { get; set; }
    public ApiClient? ApiClient { get; set; }

    public string OriginalUrl { get; set; } = string.Empty;
    public string NormalizedUrl { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;

    public ScanStatus Status { get; set; } = ScanStatus.Queued;
    public string? FailureReason { get; set; }

    public int RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public ScanVerdict Verdict { get; set; } = ScanVerdict.Unknown;
    public decimal Confidence { get; set; }

    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public UrlAnalysis? UrlAnalysis { get; set; }
    public DomainAnalysis? DomainAnalysis { get; set; }
    public DnsAnalysis? DnsAnalysis { get; set; }
    public SslAnalysis? SslAnalysis { get; set; }
    public ICollection<RedirectAnalysis> RedirectAnalyses { get; set; } = new List<RedirectAnalysis>();
    public ICollection<ThreatIntelligenceResult> ThreatIntelligenceResults { get; set; } = new List<ThreatIntelligenceResult>();
    public ICollection<BrandMatch> BrandMatches { get; set; } = new List<BrandMatch>();
    public MlPrediction? MlPrediction { get; set; }
    public ICollection<RiskFactor> RiskFactors { get; set; } = new List<RiskFactor>();
    public ICollection<ScanEvent> Events { get; set; } = new List<ScanEvent>();
}
