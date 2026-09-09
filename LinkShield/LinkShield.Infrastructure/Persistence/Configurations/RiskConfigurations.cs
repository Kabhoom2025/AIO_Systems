using LinkShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkShield.Infrastructure.Persistence.Configurations;

public class RiskRuleConfiguration : IEntityTypeConfiguration<RiskRule>
{
    public void Configure(EntityTypeBuilder<RiskRule> builder)
    {
        builder.Property(r => r.Name).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Weight).HasColumnType("decimal(5,2)");
    }
}

public class RiskFactorConfiguration : IEntityTypeConfiguration<RiskFactor>
{
    public void Configure(EntityTypeBuilder<RiskFactor> builder)
    {
        builder.Property(f => f.Description).HasMaxLength(500).IsRequired();

        builder.HasOne(f => f.RiskRule)
            .WithMany()
            .HasForeignKey(f => f.RiskRuleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
