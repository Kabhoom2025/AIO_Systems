using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NovaERP.Domain.Entities;

namespace NovaERP.Infrastructure.Data.Configurations;

public class ScheduledJobDefinitionConfiguration : IEntityTypeConfiguration<ScheduledJobDefinition>
{
    public void Configure(EntityTypeBuilder<ScheduledJobDefinition> builder)
    {
        builder.ToTable("ScheduledJobDefinitions");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Name).IsRequired().HasMaxLength(200);
        builder.Property(j => j.JobKey).IsRequired().HasMaxLength(100);
        builder.Property(j => j.CronExpression).IsRequired().HasMaxLength(50);
        builder.Property(j => j.LastRunStatus).HasMaxLength(500);
        builder.HasIndex(j => new { j.OrganizationId, j.JobKey });

        // Nullable OrganizationId (platform-wide jobs) means no cascade FK constraint here —
        // SetNull would also be wrong since we want the row to remain platform-wide; Restrict
        // simply prevents deleting an org that still has org-scoped job rows.
        builder.HasOne(j => j.Organization)
               .WithMany()
               .HasForeignKey(j => j.OrganizationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationChannelSettingsConfiguration : IEntityTypeConfiguration<NotificationChannelSettings>
{
    public void Configure(EntityTypeBuilder<NotificationChannelSettings> builder)
    {
        builder.ToTable("NotificationChannelSettings");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.OrganizationId).IsUnique();

        builder.Property(s => s.SmtpHost).HasMaxLength(200);
        builder.Property(s => s.SmtpUsername).HasMaxLength(200);
        builder.Property(s => s.SmtpPassword).HasMaxLength(500);
        builder.Property(s => s.SmtpFromEmail).HasMaxLength(200);
        builder.Property(s => s.SmtpFromName).HasMaxLength(200);
        builder.Property(s => s.SmsApiUrl).HasMaxLength(500);
        builder.Property(s => s.SmsApiKey).HasMaxLength(500);
        builder.Property(s => s.SmsSenderId).HasMaxLength(50);
        builder.Property(s => s.PushApiUrl).HasMaxLength(500);
        builder.Property(s => s.PushServerKey).HasMaxLength(500);

        builder.HasOne(s => s.Organization)
               .WithMany()
               .HasForeignKey(s => s.OrganizationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationDeliveryLogConfiguration : IEntityTypeConfiguration<NotificationDeliveryLog>
{
    public void Configure(EntityTypeBuilder<NotificationDeliveryLog> builder)
    {
        builder.ToTable("NotificationDeliveryLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Channel).IsRequired().HasMaxLength(20);
        builder.Property(l => l.Status).IsRequired().HasMaxLength(20);
        builder.Property(l => l.Error).HasMaxLength(1000);
        builder.HasIndex(l => l.NotificationId);

        builder.HasOne(l => l.Notification)
               .WithMany()
               .HasForeignKey(l => l.NotificationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
