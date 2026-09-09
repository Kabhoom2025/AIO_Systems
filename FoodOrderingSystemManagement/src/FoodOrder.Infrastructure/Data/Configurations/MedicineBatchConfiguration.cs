using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.ToTable("MedicineBatches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(b => b.PurchasePrice).HasColumnType("numeric(10,2)");

        builder.HasOne(b => b.Medicine)
               .WithMany(m => m.Batches)
               .HasForeignKey(b => b.MedicineId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
