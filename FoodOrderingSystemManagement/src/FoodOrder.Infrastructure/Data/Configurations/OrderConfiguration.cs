using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.OrderDate)
            .HasDefaultValueSql("NOW()");

        builder.Property(o => o.SubTotal)
            .HasColumnType("numeric(10,2)");

        builder.Property(o => o.Tax)
            .HasColumnType("numeric(10,2)");

        builder.Property(o => o.Discount)
            .HasColumnType("numeric(10,2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.GrandTotal)
            .HasColumnType("numeric(10,2)");

        // Stored as string ("Pending", "Completed", "Cancelled") — human-readable in DB.
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(OrderStatus.Pending);

        builder.Property(o => o.CreatedDate)
            .HasDefaultValueSql("NOW()");

        // CashierId is nullable — public/online orders have no cashier.
        // Restrict: orders must be preserved even if a cashier account is deactivated.
        builder.HasOne(o => o.Cashier)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CashierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Nullable FK — takeaway orders have no table.
        builder.HasOne(o => o.Table)
            .WithMany(t => t.Orders)
            .HasForeignKey(o => o.TableId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(o => o.OrderType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(OrderType.DineIn);

        builder.Property(o => o.DeliveryAddress).HasMaxLength(500);

        builder.Property(o => o.DeliveryCharge)
            .HasColumnType("numeric(10,2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.PointsEarned).HasDefaultValue(0);
        builder.Property(o => o.PointsRedeemed).HasDefaultValue(0);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.OrderType);

        builder.HasOne(o => o.Branch)
            .WithMany()
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.BranchId);
    }
}
