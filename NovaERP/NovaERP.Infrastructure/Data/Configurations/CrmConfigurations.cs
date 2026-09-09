using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(l => new { l.OrganizationId, l.Status });

        builder.HasOne(l => l.Organization)
               .WithMany()
               .HasForeignKey(l => l.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Owner)
               .WithMany()
               .HasForeignKey(l => l.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.ConvertedAccount)
               .WithMany()
               .HasForeignKey(l => l.ConvertedAccountId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(a => new { a.OrganizationId, a.Name });

        builder.HasOne(a => a.Organization)
               .WithMany()
               .HasForeignKey(a => a.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Owner)
               .WithMany()
               .HasForeignKey(a => a.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.OrganizationId);

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Account)
               .WithMany(a => a.Contacts)
               .HasForeignKey(c => c.AccountId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Owner)
               .WithMany()
               .HasForeignKey(c => c.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("Opportunities");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(150);
        builder.Property(o => o.Stage).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");
        builder.HasIndex(o => new { o.OrganizationId, o.Stage });

        builder.HasOne(o => o.Organization)
               .WithMany()
               .HasForeignKey(o => o.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Account)
               .WithMany(a => a.Opportunities)
               .HasForeignKey(o => o.AccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Owner)
               .WithMany()
               .HasForeignKey(o => o.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
