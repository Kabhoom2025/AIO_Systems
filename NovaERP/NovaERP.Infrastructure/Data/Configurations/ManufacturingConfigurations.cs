using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class BillOfMaterialConfiguration : IEntityTypeConfiguration<BillOfMaterial>
{
    public void Configure(EntityTypeBuilder<BillOfMaterial> builder)
    {
        builder.ToTable("BillOfMaterials");
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => new { b.OrganizationId, b.ProductId }).IsUnique();

        builder.HasOne(b => b.Organization)
               .WithMany()
               .HasForeignKey(b => b.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict — a Product with a BOM shouldn't be deletable (same "protect
        // transactional/reference history" reasoning as Opportunity.AccountId).
        builder.HasOne(b => b.Product)
               .WithMany()
               .HasForeignKey(b => b.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BomComponentConfiguration : IEntityTypeConfiguration<BomComponent>
{
    public void Configure(EntityTypeBuilder<BomComponent> builder)
    {
        builder.ToTable("BomComponents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Quantity).HasColumnType("decimal(18,4)");

        builder.HasOne(c => c.BillOfMaterial)
               .WithMany(b => b.Components)
               .HasForeignKey(c => c.BillOfMaterialId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ComponentProduct)
               .WithMany()
               .HasForeignKey(c => c.ComponentProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("ProductionOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.MoNumber).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Status).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Quantity).HasColumnType("decimal(18,4)");
        builder.HasIndex(o => new { o.OrganizationId, o.MoNumber }).IsUnique();

        builder.HasOne(o => o.Organization)
               .WithMany()
               .HasForeignKey(o => o.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Product)
               .WithMany()
               .HasForeignKey(o => o.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Warehouse)
               .WithMany()
               .HasForeignKey(o => o.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Owner)
               .WithMany()
               .HasForeignKey(o => o.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
