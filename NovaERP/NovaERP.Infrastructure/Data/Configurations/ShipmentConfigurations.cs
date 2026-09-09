using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.ShipmentNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.SourceType).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.ShipToContactName).HasMaxLength(150);
        builder.Property(s => s.ShipToEmail).HasMaxLength(200);
        builder.Property(s => s.ShipToTaxType).HasMaxLength(50);
        builder.Property(s => s.ShipToTaxCountry).HasMaxLength(100);
        builder.Property(s => s.ShipToTaxId).HasMaxLength(100);
        builder.Property(s => s.ShipFromContactName).HasMaxLength(150);
        builder.Property(s => s.ShipFromEmail).HasMaxLength(200);
        builder.HasIndex(s => new { s.OrganizationId, s.ShipmentNumber }).IsUnique();

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        // Two FKs to the same Warehouse table need two explicit, distinctly-named
        // relationships, same as StockTransfer.FromWarehouse/ToWarehouse.
        builder.HasOne(s => s.Warehouse)
               .WithMany()
               .HasForeignKey(s => s.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.DestinationWarehouse)
               .WithMany()
               .HasForeignKey(s => s.DestinationWarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SalesOrder)
               .WithMany()
               .HasForeignKey(s => s.SalesOrderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Owner)
               .WithMany()
               .HasForeignKey(s => s.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToTable("ShipmentLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Quantity).HasColumnType("decimal(18,4)");

        builder.HasOne(l => l.Shipment)
               .WithMany(s => s.Lines)
               .HasForeignKey(l => l.ShipmentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.SalesOrderLine)
               .WithMany()
               .HasForeignKey(l => l.SalesOrderLineId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ShipmentPackageConfiguration : IEntityTypeConfiguration<ShipmentPackage>
{
    public void Configure(EntityTypeBuilder<ShipmentPackage> builder)
    {
        builder.ToTable("ShipmentPackages");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.WeightKg).HasColumnType("decimal(10,3)");
        builder.Property(p => p.LengthCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.WidthCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.HeightCm).HasColumnType("decimal(10,2)");

        builder.HasOne(p => p.Shipment)
               .WithMany(s => s.Packages)
               .HasForeignKey(p => p.ShipmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShipmentPackageItemConfiguration : IEntityTypeConfiguration<ShipmentPackageItem>
{
    public void Configure(EntityTypeBuilder<ShipmentPackageItem> builder)
    {
        builder.ToTable("ShipmentPackageItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity).HasColumnType("decimal(18,4)");

        builder.HasOne(i => i.Package)
               .WithMany(p => p.Items)
               .HasForeignKey(i => i.ShipmentPackageId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
               .WithMany()
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
