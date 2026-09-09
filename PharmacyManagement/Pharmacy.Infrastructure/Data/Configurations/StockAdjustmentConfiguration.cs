using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AdjustmentType).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Reason).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasOne(a => a.Organization)
               .WithMany(o => o.StockAdjustments)
               .HasForeignKey(a => a.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Medicine)
               .WithMany()
               .HasForeignKey(a => a.MedicineId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.MedicineBatch)
               .WithMany()
               .HasForeignKey(a => a.MedicineBatchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
