using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class JobOpeningConfiguration : IEntityTypeConfiguration<JobOpening>
{
    public void Configure(EntityTypeBuilder<JobOpening> builder)
    {
        builder.ToTable("JobOpenings");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.Status).IsRequired().HasMaxLength(20);
        builder.Property(j => j.SalaryRangeFrom).HasPrecision(18, 2);
        builder.Property(j => j.SalaryRangeTo).HasPrecision(18, 2);
        builder.Property(j => j.MinExperienceYears).HasPrecision(4, 1);
        builder.Property(j => j.MaxExperienceYears).HasPrecision(4, 1);

        builder.HasOne(j => j.Organization)
               .WithMany()
               .HasForeignKey(j => j.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(j => j.Department)
               .WithMany()
               .HasForeignKey(j => j.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Designation)
               .WithMany()
               .HasForeignKey(j => j.DesignationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Stage).IsRequired().HasMaxLength(20);
        builder.Property(c => c.ExpectedSalary).HasPrecision(18, 2);
        builder.Property(c => c.OfferedSalary).HasPrecision(18, 2);
        builder.Property(c => c.TotalExperienceYears).HasPrecision(4, 1);
        builder.Property(c => c.TrackingToken).IsRequired().HasMaxLength(64);
        builder.HasIndex(c => new { c.JobOpeningId, c.Stage });
        builder.HasIndex(c => c.TrackingToken).IsUnique();

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.JobOpening)
               .WithMany(j => j.Candidates)
               .HasForeignKey(c => c.JobOpeningId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CandidateStageHistoryConfiguration : IEntityTypeConfiguration<CandidateStageHistory>
{
    public void Configure(EntityTypeBuilder<CandidateStageHistory> builder)
    {
        builder.ToTable("CandidateStageHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Stage).IsRequired().HasMaxLength(20);
        builder.Property(h => h.ChangedByName).IsRequired().HasMaxLength(200);
        builder.HasIndex(h => new { h.CandidateId, h.CreatedDate });

        builder.HasOne(h => h.Candidate)
               .WithMany(c => c.StageHistory)
               .HasForeignKey(h => h.CandidateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        builder.ToTable("Interviews");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Mode).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Status).IsRequired().HasMaxLength(20);

        builder.HasOne(i => i.Candidate)
               .WithMany(c => c.Interviews)
               .HasForeignKey(i => i.CandidateId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.InterviewerUser)
               .WithMany()
               .HasForeignKey(i => i.InterviewerUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PerformanceGoalConfiguration : IEntityTypeConfiguration<PerformanceGoal>
{
    public void Configure(EntityTypeBuilder<PerformanceGoal> builder)
    {
        builder.ToTable("PerformanceGoals");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Title).IsRequired().HasMaxLength(300);
        builder.Property(g => g.Status).IsRequired().HasMaxLength(20);
        builder.Property(g => g.Weight).HasPrecision(5, 1);

        builder.HasOne(g => g.Organization)
               .WithMany()
               .HasForeignKey(g => g.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.Employee)
               .WithMany()
               .HasForeignKey(g => g.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.ToTable("PerformanceReviews");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Period).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Recommendation).IsRequired().HasMaxLength(20);
        builder.Property(r => r.SelfRating).HasPrecision(3, 1);
        builder.Property(r => r.ManagerRating).HasPrecision(3, 1);
        builder.Property(r => r.FinalRating).HasPrecision(3, 1);
        builder.HasIndex(r => new { r.EmployeeId, r.Period }).IsUnique();

        builder.HasOne(r => r.Organization)
               .WithMany()
               .HasForeignKey(r => r.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Employee)
               .WithMany()
               .HasForeignKey(r => r.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewerUser)
               .WithMany()
               .HasForeignKey(r => r.ReviewerUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
