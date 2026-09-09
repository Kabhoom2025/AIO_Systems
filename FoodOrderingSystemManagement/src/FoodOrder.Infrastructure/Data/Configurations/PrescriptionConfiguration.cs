using FoodOrder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrder.Infrastructure.Data.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PatientName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.DoctorName).HasMaxLength(200);
        builder.Property(p => p.Status).HasMaxLength(20);

        builder.HasOne(p => p.Organization)
               .WithMany()
               .HasForeignKey(p => p.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Items)
               .WithOne(i => i.Prescription)
               .HasForeignKey(i => i.PrescriptionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
