using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

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

        builder.HasOne(b => b.Branch)
               .WithMany(br => br.MedicineBatches)
               .HasForeignKey(b => b.BranchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
