using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class FoodItemRatingConfiguration : IEntityTypeConfiguration<FoodItemRating>
{
    public void Configure(EntityTypeBuilder<FoodItemRating> builder)
    {
        builder.ToTable("FoodItemRatings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.SessionId).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Comment).HasMaxLength(500);
        builder.Property(r => r.TableNumber).HasMaxLength(20);
        builder.Property(r => r.CreatedDate).HasDefaultValueSql("NOW()");

        // One rating per session per food item
        builder.HasIndex(r => new { r.SessionId, r.FoodItemId }).IsUnique();

        builder.HasOne(r => r.FoodItem)
               .WithMany(f => f.Ratings)
               .HasForeignKey(r => r.FoodItemId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
