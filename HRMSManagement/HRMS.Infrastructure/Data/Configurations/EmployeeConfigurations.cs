using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(30);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.WorkEmail).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Gender).IsRequired().HasMaxLength(10);
        builder.Property(e => e.EmploymentType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => new { e.OrganizationId, e.EmployeeCode }).IsUnique();
        builder.HasIndex(e => new { e.OrganizationId, e.WorkEmail }).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.Ignore(e => e.FullName);

        builder.HasOne(e => e.Organization)
               .WithMany(o => o.Employees)
               .HasForeignKey(e => e.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Branch)
               .WithMany()
               .HasForeignKey(e => e.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
               .WithMany(d => d.Employees)
               .HasForeignKey(e => e.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Designation)
               .WithMany()
               .HasForeignKey(e => e.DesignationId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Shift)
               .WithMany()
               .HasForeignKey(e => e.ShiftId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Manager)
               .WithMany(e => e.DirectReports)
               .HasForeignKey(e => e.ManagerId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Type).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(d => d.Employee)
               .WithMany(e => e.Documents)
               .HasForeignKey(d => d.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeEducationConfiguration : IEntityTypeConfiguration<EmployeeEducation>
{
    public void Configure(EntityTypeBuilder<EmployeeEducation> builder)
    {
        builder.ToTable("EmployeeEducations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Degree).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Institution).IsRequired().HasMaxLength(300);

        builder.HasOne(e => e.Employee)
               .WithMany(emp => emp.Educations)
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeExperienceConfiguration : IEntityTypeConfiguration<EmployeeExperience>
{
    public void Configure(EntityTypeBuilder<EmployeeExperience> builder)
    {
        builder.ToTable("EmployeeExperiences");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Company).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);

        builder.HasOne(e => e.Employee)
               .WithMany(emp => emp.Experiences)
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeFamilyMemberConfiguration : IEntityTypeConfiguration<EmployeeFamilyMember>
{
    public void Configure(EntityTypeBuilder<EmployeeFamilyMember> builder)
    {
        builder.ToTable("EmployeeFamilyMembers");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Relationship).IsRequired().HasMaxLength(50);

        builder.HasOne(f => f.Employee)
               .WithMany(e => e.FamilyMembers)
               .HasForeignKey(f => f.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeLifecycleEventConfiguration : IEntityTypeConfiguration<EmployeeLifecycleEvent>
{
    public void Configure(EntityTypeBuilder<EmployeeLifecycleEvent> builder)
    {
        builder.ToTable("EmployeeLifecycleEvents");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.EventType).IsRequired().HasMaxLength(30);
        builder.HasIndex(l => new { l.EmployeeId, l.EventDate });

        builder.HasOne(l => l.Employee)
               .WithMany(e => e.LifecycleEvents)
               .HasForeignKey(l => l.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
