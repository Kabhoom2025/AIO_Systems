using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class DeliveryOrderConfiguration : IEntityTypeConfiguration<DeliveryOrder>
{
    public void Configure(EntityTypeBuilder<DeliveryOrder> builder)
    {
        builder.ToTable("DeliveryOrders");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(DeliveryStatus.Pending);

        builder.Property(d => d.DeliveryAddress).IsRequired().HasMaxLength(500);
        builder.Property(d => d.DeliveryCharge).HasColumnType("numeric(10,2)");
        builder.Property(d => d.CustomerPhone).HasMaxLength(20);
        builder.Property(d => d.Notes).HasMaxLength(500);
        builder.Property(d => d.ThirdPartyProvider).HasMaxLength(50);
        builder.Property(d => d.ThirdPartyTrackId).HasMaxLength(200);

        builder.HasOne(d => d.Order).WithMany()
            .HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Driver).WithMany(dr => dr.DeliveryOrders)
            .HasForeignKey(d => d.DriverId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => d.OrderId);
        builder.HasIndex(d => d.DriverId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.CreatedDate);
    }
}
