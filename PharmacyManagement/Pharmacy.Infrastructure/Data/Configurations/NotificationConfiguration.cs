using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pharmacy.Domain.Entities;

namespace Pharmacy.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).IsRequired().HasMaxLength(30);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.RelatedEntityType).HasMaxLength(30);

        builder.HasIndex(n => new { n.OrganizationId, n.IsRead });
        builder.HasIndex(n => new { n.OrganizationId, n.Type, n.RelatedEntityId });

        builder.HasOne(n => n.Organization)
               .WithMany()
               .HasForeignKey(n => n.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
