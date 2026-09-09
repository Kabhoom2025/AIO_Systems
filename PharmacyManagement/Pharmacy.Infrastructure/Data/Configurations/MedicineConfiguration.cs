using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(300);
        builder.Property(m => m.GenericName).HasMaxLength(300);
        builder.Property(m => m.Category).HasMaxLength(100);
        builder.Property(m => m.DrugSchedule).HasMaxLength(10);
        builder.Property(m => m.MRP).HasColumnType("numeric(10,2)");
        builder.Property(m => m.PurchasePrice).HasColumnType("numeric(10,2)");
        builder.Property(m => m.GstPercent).HasColumnType("numeric(5,2)");
        builder.Property(m => m.Sku).IsRequired().HasMaxLength(30);
        builder.Property(m => m.Barcode).IsRequired().HasMaxLength(30);
        builder.HasIndex(m => m.Sku).IsUnique();
        builder.HasIndex(m => m.Barcode).IsUnique();

        builder.HasOne(m => m.Organization)
               .WithMany(o => o.Medicines)
               .HasForeignKey(m => m.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
