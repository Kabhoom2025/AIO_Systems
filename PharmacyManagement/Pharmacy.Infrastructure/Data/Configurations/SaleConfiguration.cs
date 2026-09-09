using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.PaymentMethod).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Channel).IsRequired().HasMaxLength(20).HasDefaultValue("POS");
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Subtotal).HasColumnType("numeric(12,2)");
        builder.Property(s => s.DiscountAmount).HasColumnType("numeric(12,2)");
        builder.Property(s => s.TaxAmount).HasColumnType("numeric(12,2)");
        builder.Property(s => s.TotalAmount).HasColumnType("numeric(12,2)");

        builder.HasOne(s => s.Organization)
               .WithMany(o => o.Sales)
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Branch)
               .WithMany()
               .HasForeignKey(s => s.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Customer)
               .WithMany()
               .HasForeignKey(s => s.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Patient)
               .WithMany()
               .HasForeignKey(s => s.PatientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
