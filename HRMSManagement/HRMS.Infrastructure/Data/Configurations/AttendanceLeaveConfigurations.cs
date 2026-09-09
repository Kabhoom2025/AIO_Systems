using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Source).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
        builder.HasIndex(a => new { a.OrganizationId, a.Date });

        builder.HasOne(a => a.Organization)
               .WithMany()
               .HasForeignKey(a => a.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Employee)
               .WithMany()
               .HasForeignKey(a => a.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AttendanceRegularizationConfiguration : IEntityTypeConfiguration<AttendanceRegularization>
{
    public void Configure(EntityTypeBuilder<AttendanceRegularization> builder)
    {
        builder.ToTable("AttendanceRegularizations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Employee)
               .WithMany()
               .HasForeignKey(r => r.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedByUser)
               .WithMany()
               .HasForeignKey(r => r.ReviewedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
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
        builder.Property(t => t.AnnualQuota).HasPrecision(5, 1);
        builder.Property(t => t.MaxCarryForward).HasPrecision(5, 1);
        builder.HasIndex(t => new { t.OrganizationId, t.Code }).IsUnique();

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Allocated).HasPrecision(5, 1);
        builder.Property(b => b.Used).HasPrecision(5, 1);
        builder.Property(b => b.CarriedForward).HasPrecision(5, 1);
        builder.HasIndex(b => new { b.EmployeeId, b.LeaveTypeId, b.Year }).IsUnique();
        builder.Ignore(b => b.Available);

        builder.HasOne(b => b.Organization)
               .WithMany()
               .HasForeignKey(b => b.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Employee)
               .WithMany()
               .HasForeignKey(b => b.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.LeaveType)
               .WithMany()
               .HasForeignKey(b => b.LeaveTypeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Days).HasPrecision(5, 1);
        builder.HasIndex(r => new { r.OrganizationId, r.Status });

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Employee)
               .WithMany()
               .HasForeignKey(r => r.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.LeaveType)
               .WithMany()
               .HasForeignKey(r => r.LeaveTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReviewedByUser)
               .WithMany()
               .HasForeignKey(r => r.ReviewedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
