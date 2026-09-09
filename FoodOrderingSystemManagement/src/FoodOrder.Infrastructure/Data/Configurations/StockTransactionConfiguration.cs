using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.Property(t => t.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.BatchNumber).HasMaxLength(50);
        builder.Property(t => t.Supplier).HasMaxLength(150);
        builder.Property(t => t.ReferenceNumber).HasMaxLength(100);
        builder.Property(t => t.Notes).HasMaxLength(500);

        builder.Property(t => t.Quantity).HasPrecision(18, 3);
        builder.Property(t => t.StockBefore).HasPrecision(18, 3);
        builder.Property(t => t.StockAfter).HasPrecision(18, 3);

        builder.HasOne(t => t.InventoryItem)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.InventoryItemId);
        builder.HasIndex(t => t.CreatedAt);

        builder.HasOne(t => t.Branch)
            .WithMany()
            .HasForeignKey(t => t.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.BranchId);
    }
}
