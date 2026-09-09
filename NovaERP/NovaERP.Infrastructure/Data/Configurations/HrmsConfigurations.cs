using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(200);
        builder.Property(e => e.JobTitle).IsRequired().HasMaxLength(150);
        builder.Property(e => e.EmploymentType).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => new { e.OrganizationId, e.EmployeeCode }).IsUnique();

        builder.HasOne(e => e.Organization)
               .WithMany()
               .HasForeignKey(e => e.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Department)
               .WithMany()
               .HasForeignKey(e => e.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        // Self-referencing manager hierarchy — same shape as Department.ParentId/Children.
        builder.HasOne(e => e.ReportingManager)
               .WithMany(e => e.DirectReports)
               .HasForeignKey(e => e.ReportingManagerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.DefaultDaysPerYear).HasColumnType("decimal(6,2)");
        builder.HasIndex(t => new { t.OrganizationId, t.Code }).IsUnique();

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.DaysRequested).HasColumnType("decimal(6,2)");
        builder.Property(l => l.Status).IsRequired().HasMaxLength(20);

        builder.HasOne(l => l.Organization)
               .WithMany()
               .HasForeignKey(l => l.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Employee)
               .WithMany(e => e.LeaveRequests)
               .HasForeignKey(l => l.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LeaveType)
               .WithMany(t => t.LeaveRequests)
               .HasForeignKey(l => l.LeaveTypeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
