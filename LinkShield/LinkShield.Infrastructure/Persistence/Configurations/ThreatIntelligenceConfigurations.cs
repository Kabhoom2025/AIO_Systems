using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShield.Infrastructure.Persistence.Configurations;

public class ThreatIntelligenceProviderConfiguration : IEntityTypeConfiguration<ThreatIntelligenceProvider>
{
    public void Configure(EntityTypeBuilder<ThreatIntelligenceProvider> builder)
    {
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Slug).HasMaxLength(64).IsRequired();
        builder.Property(p => p.WeightMultiplier).HasColumnType("decimal(4,2)");
    }
}

public class ThreatIntelligenceResultConfiguration : IEntityTypeConfiguration<ThreatIntelligenceResult>
{
    public void Configure(EntityTypeBuilder<ThreatIntelligenceResult> builder)
    {
        builder.HasIndex(r => new { r.QueriedValue, r.ThreatIntelligenceProviderId });
        builder.HasIndex(r => r.CacheExpiresAtUtc);

        builder.HasOne(r => r.Provider)
            .WithMany(p => p.Results)
            .HasForeignKey(r => r.ThreatIntelligenceProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
