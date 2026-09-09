using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(o => o.Code).IsUnique();
    }
}

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(b => new { b.OrganizationId, b.Code }).IsUnique();

        builder.HasOne(b => b.Organization)
               .WithMany(o => o.Branches)
               .HasForeignKey(b => b.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(d => new { d.OrganizationId, d.Code }).IsUnique();

        builder.HasOne(d => d.Organization)
               .WithMany(o => o.Departments)
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Branch)
               .WithMany()
               .HasForeignKey(d => d.BranchId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Parent)
               .WithMany(d => d.Children)
               .HasForeignKey(d => d.ParentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.HeadEmployee)
               .WithMany()
               .HasForeignKey(d => d.HeadEmployeeId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.ToTable("Designations");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(30);
        builder.HasIndex(d => new { d.OrganizationId, d.Code }).IsUnique();

        builder.HasOne(d => d.Organization)
               .WithMany()
               .HasForeignKey(d => d.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.JobGrade)
               .WithMany()
               .HasForeignKey(d => d.JobGradeId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class JobGradeConfiguration : IEntityTypeConfiguration<JobGrade>
{
    public void Configure(EntityTypeBuilder<JobGrade> builder)
    {
        builder.ToTable("JobGrades");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(50);
        builder.Property(g => g.MinAnnualSalary).HasPrecision(18, 2);
        builder.Property(g => g.MaxAnnualSalary).HasPrecision(18, 2);

        builder.HasOne(g => g.Organization)
               .WithMany()
               .HasForeignKey(g => g.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(30);
        builder.Property(s => s.WeeklyOffDays).HasMaxLength(100);

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Type).IsRequired().HasMaxLength(20);
        builder.HasIndex(h => new { h.OrganizationId, h.Date });

        builder.HasOne(h => h.Organization)
               .WithMany()
               .HasForeignKey(h => h.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Branch)
               .WithMany()
               .HasForeignKey(h => h.BranchId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
