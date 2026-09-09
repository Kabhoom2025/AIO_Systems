using AIO_Systems.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIO_Systems.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Address).HasMaxLength(500);
        builder.Property(o => o.Phone).HasMaxLength(30);
        builder.Property(o => o.Email).HasMaxLength(200);
        builder.Property(o => o.LogoUrl).HasMaxLength(500);
        builder.Property(o => o.IsActive).IsRequired();
        builder.Property(o => o.Timezone).HasMaxLength(50);
        builder.Property(o => o.Currency).HasMaxLength(10);
        builder.Property(o => o.CreatedDate).IsRequired();
        builder.Property(o => o.TenantKey).IsRequired().HasMaxLength(100);

        builder.HasIndex(o => o.Name).IsUnique();
        builder.HasIndex(o => o.TenantKey).IsUnique();

        builder.HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
