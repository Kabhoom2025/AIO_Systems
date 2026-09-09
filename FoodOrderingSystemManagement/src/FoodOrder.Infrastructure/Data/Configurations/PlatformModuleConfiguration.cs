using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class PlatformModuleConfiguration : IEntityTypeConfiguration<PlatformModule>
{
    public void Configure(EntityTypeBuilder<PlatformModule> builder)
    {
        builder.ToTable("PlatformModules");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Key).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Key).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Icon).HasMaxLength(100);
        builder.Property(p => p.Color).HasMaxLength(20);
    }
}
