using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.PoNumber).IsRequired(false).HasMaxLength(20);
        builder.Property(o => o.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(o => new { o.OrganizationId, o.PoNumber }).IsUnique();
        builder.HasIndex(o => new { o.OrganizationId, o.Status });

        builder.HasOne(o => o.Organization)
               .WithMany()
               .HasForeignKey(o => o.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Vendor)
               .WithMany()
               .HasForeignKey(o => o.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.RfqRequest)
               .WithMany()
               .HasForeignKey(o => o.RfqRequestId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Owner)
               .WithMany()
               .HasForeignKey(o => o.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.ItemName).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TaxRatePercent).HasColumnType("decimal(9,4)");

        builder.HasOne(l => l.PurchaseOrder)
               .WithMany(o => o.Lines)
               .HasForeignKey(l => l.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.TaxCode)
               .WithMany()
               .HasForeignKey(l => l.TaxCodeId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
