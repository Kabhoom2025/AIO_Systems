using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AssetTag).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Category).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Condition).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Status).IsRequired().HasMaxLength(20);
        builder.Property(a => a.PurchaseCost).HasPrecision(18, 2);
        builder.HasIndex(a => new { a.OrganizationId, a.AssetTag }).IsUnique();

        builder.HasOne(a => a.Organization)
               .WithMany()
               .HasForeignKey(a => a.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Branch)
               .WithMany()
               .HasForeignKey(a => a.BranchId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AssetAllocationConfiguration : IEntityTypeConfiguration<AssetAllocation>
{
    public void Configure(EntityTypeBuilder<AssetAllocation> builder)
    {
        builder.ToTable("AssetAllocations");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Asset)
               .WithMany(x => x.Allocations)
               .HasForeignKey(a => a.AssetId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Employee)
               .WithMany()
               .HasForeignKey(a => a.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.AllocatedByUser)
               .WithMany()
               .HasForeignKey(a => a.AllocatedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ExpenseClaimConfiguration : IEntityTypeConfiguration<ExpenseClaim>
{
    public void Configure(EntityTypeBuilder<ExpenseClaim> builder)
    {
        builder.ToTable("ExpenseClaims");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Currency).IsRequired().HasMaxLength(10);
        builder.HasIndex(e => new { e.OrganizationId, e.Status });

        builder.HasOne(e => e.Organization)
               .WithMany()
               .HasForeignKey(e => e.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Employee)
               .WithMany()
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ReviewedByUser)
               .WithMany()
               .HasForeignKey(e => e.ReviewedByUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class HelpDeskTicketConfiguration : IEntityTypeConfiguration<HelpDeskTicket>
{
    public void Configure(EntityTypeBuilder<HelpDeskTicket> builder)
    {
        builder.ToTable("HelpDeskTickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Category).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Priority).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(4000);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(t => new { t.OrganizationId, t.TicketNumber }).IsUnique();
        builder.HasIndex(t => new { t.OrganizationId, t.Status });

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.RaisedByEmployee)
               .WithMany()
               .HasForeignKey(t => t.RaisedByEmployeeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.AssignedToUser)
               .WithMany()
               .HasForeignKey(t => t.AssignedToUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("TicketComments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.AuthorName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Comment).IsRequired().HasMaxLength(2000);

        builder.HasOne(c => c.Ticket)
               .WithMany(t => t.Comments)
               .HasForeignKey(c => c.TicketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AuthorUser)
               .WithMany()
               .HasForeignKey(c => c.AuthorUserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
