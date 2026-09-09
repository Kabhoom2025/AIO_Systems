using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Phone).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Email).HasMaxLength(200);
        builder.Property(d => d.VehicleNo).HasMaxLength(50);
        builder.Property(d => d.VehicleType).HasMaxLength(50);
        builder.HasOne(d => d.Organization).WithMany()
            .HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(d => d.OrganizationId);
    }
}
