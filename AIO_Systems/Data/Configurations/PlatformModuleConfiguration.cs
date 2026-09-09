using AIO_Systems.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIO_Systems.Data.Configurations;

public class PlatformModuleConfiguration : IEntityTypeConfiguration<PlatformModule>
{
    public void Configure(EntityTypeBuilder<PlatformModule> builder)
    {
        builder.ToTable("PlatformModules");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Key).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Icon).HasMaxLength(100);
        builder.Property(p => p.Color).HasMaxLength(20);
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.SortOrder).IsRequired();
        builder.Property(p => p.CreatedDate).IsRequired();

        builder.HasIndex(p => p.Key).IsUnique();

        // Intentionally NO HasData seed here: PlatformModules are populated by a one-time SQL
        // migration script copied from the existing FoodOrderDB, preserving exact primary key IDs
        // (1=restaurant, 2=pharmacy, 3=hr_management, 4=retail, 5=finance). Seeding here would
        // conflict with that script.
    }
}
