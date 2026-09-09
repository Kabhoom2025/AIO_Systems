using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Price)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(oi => oi.LineTotal)
            .HasColumnType("numeric(10,2)");

        // Cascade: order items are deleted when their parent order is deleted.
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: a food item cannot be force-deleted while order history references it.
        builder.HasOne(oi => oi.FoodItem)
            .WithMany(f => f.OrderItems)
            .HasForeignKey(oi => oi.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
