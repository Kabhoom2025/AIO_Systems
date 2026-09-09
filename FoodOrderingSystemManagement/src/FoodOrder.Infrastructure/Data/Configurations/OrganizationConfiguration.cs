using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.Property(x => x.Timezone).HasMaxLength(50);
        b.Property(x => x.Currency).HasMaxLength(10);
        b.HasIndex(x => x.Name).IsUnique();
        b.HasMany(x => x.Users).WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}
