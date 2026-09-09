using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShield.Infrastructure.Persistence.Configurations;

public class DomainAnalysisConfiguration : IEntityTypeConfiguration<DomainAnalysis>
{
    public void Configure(EntityTypeBuilder<DomainAnalysis> builder)
    {
        builder.Property(d => d.Domain).HasMaxLength(255).IsRequired();
        builder.Property(d => d.RegisteredDomain).HasMaxLength(255).IsRequired();
        builder.HasIndex(d => d.RegisteredDomain);
    }
}

public class RedirectAnalysisConfiguration : IEntityTypeConfiguration<RedirectAnalysis>
{
    public void Configure(EntityTypeBuilder<RedirectAnalysis> builder)
    {
        builder.Property(r => r.FromUrl).HasMaxLength(2048).IsRequired();
        builder.Property(r => r.ToUrl).HasMaxLength(2048).IsRequired();
        builder.HasIndex(r => new { r.UrlScanId, r.HopIndex }).IsUnique();
    }
}
