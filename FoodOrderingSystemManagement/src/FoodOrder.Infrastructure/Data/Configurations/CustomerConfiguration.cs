using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.LoyaltyPoints).HasDefaultValue(0);
        builder.Property(c => c.TotalSpend).HasColumnType("numeric(12,2)").HasDefaultValue(0);
        builder.Property(c => c.TotalVisits).HasDefaultValue(0);
        builder.Property(c => c.CreatedDate).IsRequired();

        builder.Ignore(c => c.Tier);

        builder.HasOne(c => c.Organization)
            .WithMany()
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique per-organization, not globally — two different chains may have
        // customers sharing a phone number.
        builder.HasIndex(c => new { c.OrganizationId, c.Phone }).IsUnique();

        builder.HasMany(c => c.Orders)
               .WithOne(o => o.Customer)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.PointsTransactions)
               .WithOne(pt => pt.Customer)
               .HasForeignKey(pt => pt.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
