using AIO_Systems.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIO_Systems.Data.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.OrganizationId).IsRequired();
        builder.Property(l => l.Plan).IsRequired();
        builder.Property(l => l.Status).IsRequired();
        builder.Property(l => l.ExpiryDate).IsRequired();
        builder.Property(l => l.MaxUsers).IsRequired();
        builder.Property(l => l.CreatedDate).IsRequired();

        builder.HasIndex(l => l.OrganizationId);

        builder.HasOne(l => l.Organization)
            .WithMany()
            .HasForeignKey(l => l.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
