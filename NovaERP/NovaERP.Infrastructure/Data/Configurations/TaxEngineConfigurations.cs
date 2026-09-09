using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class TaxCodeConfiguration : IEntityTypeConfiguration<TaxCode>
{
    public void Configure(EntityTypeBuilder<TaxCode> builder)
    {
        builder.ToTable("TaxCodes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => new { t.OrganizationId, t.Code }).IsUnique();

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TaxComponentConfiguration : IEntityTypeConfiguration<TaxComponent>
{
    public void Configure(EntityTypeBuilder<TaxComponent> builder)
    {
        builder.ToTable("TaxComponents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
        builder.Property(c => c.RatePercent).HasColumnType("decimal(9,4)");

        builder.HasOne(c => c.TaxCode)
               .WithMany(t => t.Components)
               .HasForeignKey(c => c.TaxCodeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
