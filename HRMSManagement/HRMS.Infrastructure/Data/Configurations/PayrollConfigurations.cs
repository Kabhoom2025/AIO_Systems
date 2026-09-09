using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("SalaryComponents");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Type).IsRequired().HasMaxLength(20);
        builder.Property(c => c.CalcType).IsRequired().HasMaxLength(20);
        builder.Property(c => c.DefaultValue).HasPrecision(18, 2);
        builder.HasIndex(c => new { c.OrganizationId, c.Code }).IsUnique();

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeSalaryConfiguration : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.ToTable("EmployeeSalaries");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.AnnualCtc).HasPrecision(18, 2);
        builder.Property(s => s.MonthlyGross).HasPrecision(18, 2);
        builder.Property(s => s.Currency).IsRequired().HasMaxLength(10);
        builder.HasIndex(s => new { s.EmployeeId, s.EffectiveFrom });

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Employee)
               .WithMany()
               .HasForeignKey(s => s.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeSalaryItemConfiguration : IEntityTypeConfiguration<EmployeeSalaryItem>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryItem> builder)
    {
        builder.ToTable("EmployeeSalaryItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.MonthlyAmount).HasPrecision(18, 2);

        builder.HasOne(i => i.EmployeeSalary)
               .WithMany(s => s.Items)
               .HasForeignKey(i => i.EmployeeSalaryId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.SalaryComponent)
               .WithMany()
               .HasForeignKey(i => i.SalaryComponentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);
        builder.Property(r => r.TotalGross).HasPrecision(18, 2);
        builder.Property(r => r.TotalDeductions).HasPrecision(18, 2);
        builder.Property(r => r.TotalNet).HasPrecision(18, 2);
        builder.HasIndex(r => new { r.OrganizationId, r.Year, r.Month }).IsUnique();

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ProcessedByUser)
               .WithMany()
               .HasForeignKey(r => r.ProcessedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(20);
        builder.Property(p => p.GrossEarnings).HasPrecision(18, 2);
        builder.Property(p => p.TotalDeductions).HasPrecision(18, 2);
        builder.Property(p => p.NetPay).HasPrecision(18, 2);
        builder.Property(p => p.WorkingDays).HasPrecision(5, 1);
        builder.Property(p => p.PresentDays).HasPrecision(5, 1);
        builder.Property(p => p.PaidLeaveDays).HasPrecision(5, 1);
        builder.Property(p => p.LopDays).HasPrecision(5, 1);
        builder.HasIndex(p => new { p.PayrollRunId, p.EmployeeId }).IsUnique();

        builder.HasOne(p => p.PayrollRun)
               .WithMany(r => r.Payslips)
               .HasForeignKey(p => p.PayrollRunId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Employee)
               .WithMany()
               .HasForeignKey(p => p.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayslipItemConfiguration : IEntityTypeConfiguration<PayslipItem>
{
    public void Configure(EntityTypeBuilder<PayslipItem> builder)
    {
        builder.ToTable("PayslipItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ComponentName).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Type).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Amount).HasPrecision(18, 2);

        builder.HasOne(i => i.Payslip)
               .WithMany(p => p.Items)
               .HasForeignKey(i => i.PayslipId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
