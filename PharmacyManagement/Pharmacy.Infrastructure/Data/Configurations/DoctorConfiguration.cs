using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.RegistrationNumber).HasMaxLength(50);
        builder.Property(d => d.Specialty).HasMaxLength(100);
        builder.Property(d => d.Phone).HasMaxLength(30);
        builder.Property(d => d.Email).HasMaxLength(200);
        builder.Property(d => d.HospitalName).HasMaxLength(200);

        builder.HasOne(d => d.Organization)
               .WithMany(o => o.Doctors)
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
