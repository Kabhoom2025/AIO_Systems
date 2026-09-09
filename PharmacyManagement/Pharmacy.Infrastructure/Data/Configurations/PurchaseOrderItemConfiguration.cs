using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(10,2)");

        builder.HasOne(i => i.PurchaseOrder)
               .WithMany(p => p.Items)
               .HasForeignKey(i => i.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Medicine)
               .WithMany()
               .HasForeignKey(i => i.MedicineId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
