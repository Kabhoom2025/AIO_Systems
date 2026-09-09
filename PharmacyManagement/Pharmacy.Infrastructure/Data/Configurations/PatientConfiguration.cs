using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Gender).HasMaxLength(20);
        builder.Property(p => p.BloodGroup).HasMaxLength(10);
        builder.Property(p => p.Allergies).HasMaxLength(1000);
        builder.Property(p => p.ChronicDiseases).HasMaxLength(1000);
        builder.Property(p => p.EmergencyContactName).HasMaxLength(200);
        builder.Property(p => p.EmergencyContactPhone).HasMaxLength(30);
        builder.Property(p => p.InsuranceProvider).HasMaxLength(200);
        builder.Property(p => p.InsurancePolicyNumber).HasMaxLength(100);

        builder.HasOne(p => p.Organization)
               .WithMany(o => o.Patients)
               .HasForeignKey(p => p.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
