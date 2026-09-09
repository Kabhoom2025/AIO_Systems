using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Address).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Status).IsRequired().HasMaxLength(20);
        builder.Property(d => d.OtpCode).IsRequired().HasMaxLength(10);
        builder.Property(d => d.DeliveryCharge).HasColumnType("numeric(10,2)");
        builder.Property(d => d.Notes).HasMaxLength(1000);

        builder.HasOne(d => d.Organization)
               .WithMany(o => o.Deliveries)
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Branch)
               .WithMany()
               .HasForeignKey(d => d.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Sale)
               .WithMany()
               .HasForeignKey(d => d.SaleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Customer)
               .WithMany()
               .HasForeignKey(d => d.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.DeliveryStaff)
               .WithMany()
               .HasForeignKey(d => d.DeliveryStaffId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
