using AIO_Systems.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIO_Systems.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.RoleName).IsUnique();

        // Intentionally NO HasData seed here: Roles are populated by a one-time SQL migration
        // script copied from the existing FoodOrderDB, preserving exact primary key IDs
        // (1=SuperAdmin, 2=Admin, 3=Cashier, 4=Waiter, 5=InventoryManager). Seeding here would
        // conflict with that script.
    }
}
