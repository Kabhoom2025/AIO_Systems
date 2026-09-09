using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PoNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(30);
        builder.Property(p => p.TotalAmount).HasColumnType("numeric(12,2)");
        builder.Property(p => p.Notes).HasMaxLength(1000);

        builder.HasOne(p => p.Organization)
               .WithMany(o => o.PurchaseOrders)
               .HasForeignKey(p => p.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Supplier)
               .WithMany(s => s.PurchaseOrders)
               .HasForeignKey(p => p.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
