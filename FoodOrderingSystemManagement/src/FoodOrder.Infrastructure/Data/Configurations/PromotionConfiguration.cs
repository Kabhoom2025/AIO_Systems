using FoodOrder.Domain.Entities;
using FoodOrder.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Description).HasMaxLength(500);

        builder.Property(p => p.PromotionType)
            .HasConversion<string>().HasMaxLength(30)
            .HasDefaultValue(PromotionType.Coupon);

        builder.Property(p => p.DiscountType)
            .HasConversion<string>().HasMaxLength(30)
            .HasDefaultValue(DiscountType.Percentage);

        builder.Property(p => p.DiscountValue).HasColumnType("numeric(10,2)");
        builder.Property(p => p.Code).HasMaxLength(50);
        builder.Property(p => p.MinOrderValue).HasColumnType("numeric(10,2)");
        builder.Property(p => p.MaxDiscount).HasColumnType("numeric(10,2)");
        builder.Property(p => p.GiftCardBalance).HasColumnType("numeric(10,2)");
        builder.Property(p => p.GiftCardUsed).HasColumnType("numeric(10,2)").HasDefaultValue(0m);
        builder.Property(p => p.SpendThreshold).HasColumnType("numeric(10,2)");
        builder.Property(p => p.PointsMultiplierVal).HasColumnType("numeric(6,2)");

        builder.Property(p => p.HappyHourStart).HasMaxLength(10);
        builder.Property(p => p.HappyHourEnd).HasMaxLength(10);
        builder.Property(p => p.HappyHourDays).HasMaxLength(100);
        builder.Property(p => p.ApplicableItemIds).HasMaxLength(1000);
        builder.Property(p => p.BannerColor).HasMaxLength(20);
        builder.Property(p => p.BadgeIcon).HasMaxLength(10);

        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.IsPublic).HasDefaultValue(true);
        builder.Property(p => p.UsedCount).HasDefaultValue(0);

        builder.Property(p => p.CreatedDate).HasDefaultValueSql("NOW()");

        builder.HasIndex(p => p.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
        builder.HasIndex(p => p.PromotionType);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.OrganizationId);

        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
