using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Code).IsRequired().HasMaxLength(20);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(150);
        builder.Property(v => v.Model).HasMaxLength(100);
        builder.Property(v => v.CapacityKg).HasColumnType("decimal(18,2)");
        builder.Property(v => v.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(v => new { v.OrganizationId, v.Code }).IsUnique();

        builder.HasOne(v => v.Organization)
               .WithMany()
               .HasForeignKey(v => v.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.CurrentWarehouse)
               .WithMany()
               .HasForeignKey(v => v.CurrentWarehouseId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DeliveryLoadConfiguration : IEntityTypeConfiguration<DeliveryLoad>
{
    public void Configure(EntityTypeBuilder<DeliveryLoad> builder)
    {
        builder.ToTable("DeliveryLoads");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LoadNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(l => new { l.OrganizationId, l.LoadNumber }).IsUnique();

        builder.HasOne(l => l.Organization)
               .WithMany()
               .HasForeignKey(l => l.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Vehicle)
               .WithMany(v => v.DeliveryLoads)
               .HasForeignKey(l => l.VehicleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Warehouse)
               .WithMany()
               .HasForeignKey(l => l.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        // SetNull, not Cascade — deleting a DeliveryLoad (before dispatch) shouldn't delete the
        // Shipments assigned to it, just free them back up to be assigned elsewhere.
        builder.HasMany(l => l.Shipments)
               .WithOne(s => s.DeliveryLoad)
               .HasForeignKey(s => s.DeliveryLoadId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
