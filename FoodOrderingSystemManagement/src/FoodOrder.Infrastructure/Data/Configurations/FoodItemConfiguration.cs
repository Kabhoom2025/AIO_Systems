using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("FoodItems");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ItemName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(f => f.Description)
            .HasMaxLength(500);

        builder.Property(f => f.Image)
            .HasMaxLength(500);

        builder.Property(f => f.Price)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(f => f.IsAvailable)
            .HasDefaultValue(true);

        builder.Property(f => f.Barcode)
            .HasMaxLength(50);

        builder.HasIndex(f => f.Barcode)
            .IsUnique()
            .HasFilter("\"Barcode\" IS NOT NULL");

        builder.Property(f => f.CreatedDate)
            .HasDefaultValueSql("NOW()");

        // Restrict: deleting a category must not silently remove all its food items.
        builder.HasOne(f => f.Category)
            .WithMany(c => c.FoodItems)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Branch)
            .WithMany()
            .HasForeignKey(f => f.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.BranchId);
    }
}
