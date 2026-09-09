using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShield.Infrastructure.Persistence.Configurations;

public class BrandProfileConfiguration : IEntityTypeConfiguration<BrandProfile>
{
    public void Configure(EntityTypeBuilder<BrandProfile> builder)
    {
        builder.HasIndex(b => b.OfficialDomain).IsUnique();
        builder.Property(b => b.BrandName).HasMaxLength(100).IsRequired();
        builder.Property(b => b.OfficialDomain).HasMaxLength(255).IsRequired();
    }
}

public class BrandMatchConfiguration : IEntityTypeConfiguration<BrandMatch>
{
    public void Configure(EntityTypeBuilder<BrandMatch> builder)
    {
        builder.Property(m => m.MatchedDomain).HasMaxLength(255).IsRequired();

        builder.HasOne(m => m.BrandProfile)
            .WithMany(b => b.Matches)
            .HasForeignKey(m => m.BrandProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
