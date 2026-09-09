using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(10,2)");
        builder.Property(i => i.DiscountAmount).HasColumnType("numeric(10,2)");
        builder.Property(i => i.GstPercent).HasColumnType("numeric(5,2)");
        builder.Property(i => i.LineTotal).HasColumnType("numeric(12,2)");

        builder.HasOne(i => i.Sale)
               .WithMany(s => s.Items)
               .HasForeignKey(i => i.SaleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Medicine)
               .WithMany()
               .HasForeignKey(i => i.MedicineId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.MedicineBatch)
               .WithMany()
               .HasForeignKey(i => i.MedicineBatchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
