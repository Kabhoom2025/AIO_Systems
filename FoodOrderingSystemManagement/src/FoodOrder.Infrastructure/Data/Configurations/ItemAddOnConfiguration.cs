using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class ItemAddOnConfiguration : IEntityTypeConfiguration<ItemAddOn>
{
    public void Configure(EntityTypeBuilder<ItemAddOn> builder)
    {
        builder.ToTable("ItemAddOns");

        builder.HasKey(ia => new { ia.FoodItemId, ia.AddOnId });

        builder.HasOne(ia => ia.FoodItem)
            .WithMany(f => f.ItemAddOns)
            .HasForeignKey(ia => ia.FoodItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ia => ia.AddOn)
            .WithMany(a => a.ItemAddOns)
            .HasForeignKey(ia => ia.AddOnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
