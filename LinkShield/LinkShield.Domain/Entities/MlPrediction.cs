using LinkShield.Domain.Common;

namespace LinkShield.Domain.Entities;

public class MlPrediction : BaseEntity
{
    public Guid UrlScanId { get; set; }
    public UrlScan UrlScan { get; set; } = null!;

    public string Prediction { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public string FeaturesJson { get; set; } = "{}";
    public bool RequestSucceeded { get; set; }
    public string? ErrorMessage { get; set; }
    public int ResponseTimeMs { get; set; }
}
