using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShield.Infrastructure.Persistence.Configurations;

public class UrlScanConfiguration : IEntityTypeConfiguration<UrlScan>
{
    public void Configure(EntityTypeBuilder<UrlScan> builder)
    {
        builder.Property(s => s.OriginalUrl).HasMaxLength(2048).IsRequired();
        builder.Property(s => s.NormalizedUrl).HasMaxLength(2048).IsRequired();
        builder.Property(s => s.CorrelationId).HasMaxLength(64);
        builder.Property(s => s.Confidence).HasColumnType("decimal(5,2)");

        builder.HasIndex(s => s.NormalizedUrl);
        builder.HasIndex(s => s.CreatedAtUtc);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.Verdict);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Scans)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.ApiClient)
            .WithMany()
            .HasForeignKey(s => s.ApiClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.UrlAnalysis)
            .WithOne(a => a.UrlScan)
            .HasForeignKey<UrlAnalysis>(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.DomainAnalysis)
            .WithOne(a => a.UrlScan)
            .HasForeignKey<DomainAnalysis>(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.DnsAnalysis)
            .WithOne(a => a.UrlScan)
            .HasForeignKey<DnsAnalysis>(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SslAnalysis)
            .WithOne(a => a.UrlScan)
            .HasForeignKey<SslAnalysis>(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.MlPrediction)
            .WithOne(a => a.UrlScan)
            .HasForeignKey<MlPrediction>(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.RedirectAnalyses)
            .WithOne(a => a.UrlScan)
            .HasForeignKey(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.ThreatIntelligenceResults)
            .WithOne(a => a.UrlScan)
            .HasForeignKey(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.BrandMatches)
            .WithOne(a => a.UrlScan)
            .HasForeignKey(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.RiskFactors)
            .WithOne(a => a.UrlScan)
            .HasForeignKey(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Events)
            .WithOne(a => a.UrlScan)
            .HasForeignKey(a => a.UrlScanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
