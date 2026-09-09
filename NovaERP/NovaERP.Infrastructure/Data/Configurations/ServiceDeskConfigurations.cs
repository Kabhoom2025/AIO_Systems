using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
{
    public void Configure(EntityTypeBuilder<TicketCategory> builder)
    {
        builder.ToTable("TicketCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => new { c.OrganizationId, c.Code }).IsUnique();

        builder.HasOne(c => c.Organization)
               .WithMany()
               .HasForeignKey(c => c.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ServiceTicketConfiguration : IEntityTypeConfiguration<ServiceTicket>
{
    public void Configure(EntityTypeBuilder<ServiceTicket> builder)
    {
        builder.ToTable("ServiceTickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Priority).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(20);
        builder.HasIndex(t => new { t.OrganizationId, t.TicketNumber }).IsUnique();

        builder.HasOne(t => t.Organization)
               .WithMany()
               .HasForeignKey(t => t.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Category)
               .WithMany(c => c.Tickets)
               .HasForeignKey(t => t.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Requester)
               .WithMany()
               .HasForeignKey(t => t.RequesterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedTo)
               .WithMany()
               .HasForeignKey(t => t.AssignedToId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
