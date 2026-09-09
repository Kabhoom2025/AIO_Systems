using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.UnitOfMeasure).IsRequired().HasMaxLength(20);
        builder.Property(p => p.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(p => p.WeightKg).HasColumnType("decimal(18,4)");
        builder.HasIndex(p => new { p.OrganizationId, p.Sku }).IsUnique();

        builder.HasOne(p => p.Organization)
               .WithMany()
               .HasForeignKey(p => p.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MovementType).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(m => m.EntityType).HasMaxLength(50);
        builder.HasIndex(m => new { m.OrganizationId, m.ProductId });

        builder.HasOne(m => m.Organization)
               .WithMany()
               .HasForeignKey(m => m.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade — a Product with movement history shouldn't be deletable
        // (the same "protect transactional history" reasoning as Opportunity.AccountId).
        builder.HasOne(m => m.Product)
               .WithMany(p => p.Movements)
               .HasForeignKey(m => m.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // SetNull, not Restrict — WarehouseId is an optional retrofit (same "defer the FK"
        // reasoning as ProductId on SalesOrderLine/PurchaseOrderLine); PO/SO-generated movements
        // don't set one yet.
        builder.HasOne(m => m.Warehouse)
               .WithMany(w => w.Movements)
               .HasForeignKey(m => m.WarehouseId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
