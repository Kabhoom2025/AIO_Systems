using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectFlowAI.Domain.Entities;

namespace ProjectFlowAI.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(300);
        builder.Property(n => n.Body).IsRequired();
        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        builder.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Channel).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(p => new { p.UserId, p.Channel }).IsUnique();

        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrganizationIntegrationSettingsConfiguration : IEntityTypeConfiguration<OrganizationIntegrationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationIntegrationSettings> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.OrganizationId).IsUnique();

        builder.HasOne(s => s.Organization).WithMany().HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
