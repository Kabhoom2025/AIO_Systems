using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("PharmacyOrganizations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.LicenseNo).HasMaxLength(100);
        builder.Property(o => o.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("INR");
        builder.Property(o => o.GstNumber).HasMaxLength(30);
        builder.Property(o => o.InvoiceNumberPrefix).IsRequired().HasMaxLength(20).HasDefaultValue("INV");
    }
}
