using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("Medicines");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.GenericName).HasMaxLength(200);
        builder.Property(m => m.Category).HasMaxLength(100);
        builder.Property(m => m.DrugSchedule).HasMaxLength(10);
        builder.Property(m => m.PackType).HasMaxLength(50);
        builder.Property(m => m.PackSize).HasMaxLength(100);
        builder.Property(m => m.Unit).HasMaxLength(50);
        builder.Property(m => m.Manufacturer).HasMaxLength(200);
        builder.Property(m => m.MRP).HasColumnType("numeric(10,2)");
        builder.Property(m => m.PurchasePrice).HasColumnType("numeric(10,2)");
        builder.Property(m => m.GstPercent).HasColumnType("numeric(5,2)");

        builder.HasOne(m => m.Organization)
               .WithMany()
               .HasForeignKey(m => m.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
